using Microsoft.EntityFrameworkCore;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _context;

    public ClaimRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Claims.FindAsync(new object[] { id }, ct);
    }

    public async Task<Claim?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct)
    {
        return await _context.Claims
            .Include(c => c.LossEvent)
            .Include(c => c.Parties)
            .Include(c => c.RiskObjects)
            .Include(c => c.ReserveComponents)
                .ThenInclude(r => r.History)
            .Include(c => c.Documents)
            .Include(c => c.AuditLogs)
            .FirstOrDefaultAsync(c => c.ClaimId == id, ct);
    }

    public async Task<(List<Claim> Items, int TotalCount)> ListAsync(
        Guid organizationId,
        ClaimStatus? status = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null,
        Guid? assignedHandlerId = null,
        string? causeOfLossCode = null,
        Guid? policyId = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = _context.Claims.AsQueryable()
            .Where(c => c.OrganizationEntityId == organizationId);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(c => c.LossEvent.LossDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(c => c.LossEvent.LossDate <= dateTo.Value);
        }

        if (assignedHandlerId.HasValue)
        {
            query = query.Where(c => c.AssignedHandlerId == assignedHandlerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(causeOfLossCode))
        {
            query = query.Where(c => c.LossEvent.CauseOfLossCode == causeOfLossCode);
        }

        if (policyId.HasValue)
        {
            query = query.Where(c => c.PolicyId == policyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.ClaimNumber.Contains(searchTerm) || 
                                     c.ClientName.Contains(searchTerm) || 
                                     c.PolicyNumber.Contains(searchTerm));
        }

        int totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(c => c.LossEvent)
            .Include(c => c.ReserveComponents)
            .OrderByDescending(c => c.ReportedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task AddAsync(Claim claim, CancellationToken ct)
    {
        return _context.Claims.AddAsync(claim, ct).AsTask();
    }

    public void Update(Claim claim)
    {
        _context.Claims.Update(claim);
    }

    public void Remove(Claim claim)
    {
        _context.Claims.Remove(claim);
    }

    public void Delete(Claim claim)
    {
        _context.Claims.Remove(claim); // Will be soft deleted in SaveChangesAsync
    }
}
