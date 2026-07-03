using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Repositories;

public class CauseOfLossCodeRepository : ICauseOfLossCodeRepository
{
    private readonly ClaimsDbContext _context;

    public CauseOfLossCodeRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<List<CauseOfLossCode>> GetAllActiveAsync(CancellationToken ct)
    {
        return await _context.CauseOfLossCodes
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<CauseOfLossCode?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await _context.CauseOfLossCodes
            .FirstOrDefaultAsync(c => c.Code == code, ct);
    }
}
