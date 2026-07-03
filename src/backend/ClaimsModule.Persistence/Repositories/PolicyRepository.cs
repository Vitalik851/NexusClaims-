using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly ClaimsDbContext _context;

    public PolicyRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Policies.FindAsync(new object[] { id }, ct);
    }

    public async Task<Policy?> GetByIdWithCoverageAsync(Guid id, CancellationToken ct)
    {
        // For simulated data, coverageTypes are already loaded as a column on the entity
        return await GetByIdAsync(id, ct);
    }

    public async Task<List<Policy>> SearchAsync(string query, Guid organizationId, CancellationToken ct = default)
    {
        var dbQuery = _context.Policies.Where(p => p.OrganizationEntityId == organizationId);

        if (string.IsNullOrWhiteSpace(query))
        {
            return await dbQuery.Take(10).ToListAsync(ct);
        }

        return await dbQuery
            .Where(p => p.PolicyNumber.Contains(query) || p.ClientName.Contains(query))
            .Take(10)
            .ToListAsync(ct);
    }
}
