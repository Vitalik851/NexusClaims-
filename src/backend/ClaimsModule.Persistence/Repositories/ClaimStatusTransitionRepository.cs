using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Repositories;

public class ClaimStatusTransitionRepository : IClaimStatusTransitionRepository
{
    private readonly ClaimsDbContext _context;

    public ClaimStatusTransitionRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClaimStatusTransition>> GetAllowedTransitionsAsync(ClaimStatus fromStatus, CancellationToken ct)
    {
        return await _context.ClaimStatusTransitions
            .Where(t => t.FromStatus == fromStatus)
            .ToListAsync(ct);
    }

    public async Task<List<ClaimStatusTransition>> GetAllTransitionsAsync(CancellationToken ct = default)
    {
        return await _context.ClaimStatusTransitions.ToListAsync(ct);
    }
}
