using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Repositories;

public class ReserveRepository : IReserveRepository
{
    private readonly ClaimsDbContext _context;

    public ReserveRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClaimReserveComponent>> GetByClaimIdAsync(Guid claimId, CancellationToken ct)
    {
        return await _context.ClaimReserveComponents
            .Include(r => r.History)
            .Where(r => r.ClaimId == claimId)
            .ToListAsync(ct);
    }

    public async Task<List<ReserveHistory>> GetHistoryByClaimIdAsync(Guid claimId, CancellationToken ct)
    {
        return await _context.ReserveHistories
            .Where(h => h.ClaimId == claimId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<ReserveHistory?> GetPendingByIdAsync(Guid historyId, CancellationToken ct)
    {
        return await _context.ReserveHistories
            .Include(h => h.ReserveComponent)
            .FirstOrDefaultAsync(h => h.ReserveHistoryId == historyId, ct);
    }

    public Task AddComponentAsync(ClaimReserveComponent component, CancellationToken ct)
    {
        return _context.ClaimReserveComponents.AddAsync(component, ct).AsTask();
    }

    public Task AddHistoryAsync(ReserveHistory history, CancellationToken ct)
    {
        return _context.ReserveHistories.AddAsync(history, ct).AsTask();
    }

    public void UpdateComponent(ClaimReserveComponent component)
    {
        _context.ClaimReserveComponents.Update(component);
    }

    public void UpdateHistory(ReserveHistory history)
    {
        _context.ReserveHistories.Update(history);
    }
}
