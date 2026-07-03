using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ClaimsDbContext _context;

    public AuditLogService(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        Guid claimId,
        string eventType,
        string description,
        string? oldValue,
        string? newValue,
        Guid? relatedEntityId,
        string? relatedEntityType,
        Guid? userId,
        Guid? correlationId,
        CancellationToken ct)
    {
        var log = new ClaimAuditLog
        {
            ClaimId = claimId,
            EventType = eventType,
            Description = description,
            OldValue = oldValue,
            NewValue = newValue,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CorrelationId = correlationId,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId
        };

        await _context.ClaimAuditLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }
}
