using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Infrastructure.Jobs;

public class SlaMonitoringJob : ISlaMonitoringJob
{
    private readonly ClaimsDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SlaMonitoringJob> _logger;

    public SlaMonitoringJob(
        ClaimsDbContext context,
        IAuditLogService auditLogService,
        ILogger<SlaMonitoringJob> logger)
    {
        _context = context;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("SLA Monitoring Job started scanning claims.");

        var thresholdTime = DateTimeOffset.UtcNow.AddHours(-48);

        // Find claims in Draft or Open status that haven't been updated for > 48 hours
        var staleClaims = await _context.Claims
            .Where(c => (c.Status == ClaimStatus.Draft || c.Status == ClaimStatus.Open) &&
                        (c.UpdatedAt.HasValue ? c.UpdatedAt.Value < thresholdTime : c.CreatedAt < thresholdTime))
            .ToListAsync(ct);

        _logger.LogInformation("Found {Count} stale claims for SLA verification.", staleClaims.Count);

        foreach (var claim in staleClaims)
        {
            // Check if there was an SLA_BREACH_DETECTED audit entry in the last 24 hours
            var lastBreachTime = DateTimeOffset.UtcNow.AddHours(-24);
            var alreadyBreachedRecently = await _context.ClaimAuditLogs
                .AnyAsync(l => l.ClaimId == claim.ClaimId &&
                               l.EventType == "SLA_BREACH_DETECTED" &&
                               l.CreatedAt >= lastBreachTime, ct);

            if (alreadyBreachedRecently)
            {
                _logger.LogDebug("Claim {ClaimNumber} already has SLA breach logged within the last 24 hours. Skipping.", claim.ClaimNumber);
                continue;
            }

            // Write SLA breach event
            await _auditLogService.LogAsync(
                claim.ClaimId,
                "SLA_BREACH_DETECTED",
                "Claim has not been updated in 48 hours",
                oldValue: null,
                newValue: null,
                relatedEntityId: null,
                relatedEntityType: null,
                userId: null,
                correlationId: null,
                ct);

            _logger.LogWarning("Logged SLA Breach for Claim {ClaimNumber}.", claim.ClaimNumber);
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("SLA Monitoring Job completed.");
    }
}
