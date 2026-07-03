using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Infrastructure.Jobs;

public interface IGeneralLedgerPostJob
{
    Task PostAsync(Guid reserveHistoryId, CancellationToken ct);
}

public class GeneralLedgerPostJob : IGeneralLedgerPostJob
{
    private readonly ClaimsDbContext _context;
    private readonly ILogger<GeneralLedgerPostJob> _logger;

    public GeneralLedgerPostJob(ClaimsDbContext context, ILogger<GeneralLedgerPostJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task PostAsync(Guid reserveHistoryId, CancellationToken ct)
    {
        _logger.LogInformation("Starting General Ledger posting job for ReserveHistoryId: {Id}", reserveHistoryId);

        // 1. Fetch the transaction details
        var history = await _context.ReserveHistories
            .Include(h => h.ReserveComponent)
            .FirstOrDefaultAsync(h => h.ReserveHistoryId == reserveHistoryId, ct);

        if (history == null)
        {
            _logger.LogError("ReserveHistoryId: {Id} not found in database. GL Posting aborted.", reserveHistoryId);
            return;
        }

        if (history.PostingStatus == PostingStatus.Posted)
        {
            _logger.LogWarning("ReserveHistoryId: {Id} is already posted to GL. Skipping.", reserveHistoryId);
            return;
        }

        try
        {
            // 2. Simulate external GL API network latency (3 seconds)
            await Task.Delay(3000, ct);

            // 3. Mark as Posted (with idempotency check)
            history.PostingStatus = PostingStatus.Posted;
            history.PostingJobId = Guid.NewGuid(); // Save external transaction ID reference

            _context.ReserveHistories.Update(history);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully posted transaction of {Amount:C} for ReserveComponent {Component} on Claim {ClaimId} to General Ledger. Job ID: {JobId}", 
                history.Amount, history.ReserveComponent.Component, history.ClaimId, history.PostingJobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to post transaction to GL for ReserveHistoryId: {Id}", reserveHistoryId);
            
            history.PostingStatus = PostingStatus.Failed;
            _context.ReserveHistories.Update(history);
            await _context.SaveChangesAsync(ct);
        }
    }
}
