using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Persistence.Context;

namespace ClaimsModule.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ClaimsDbContext _context;

    public AuditLogRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public IQueryable<ClaimAuditLog> GetQueryableByClaimId(Guid claimId)
    {
        return _context.ClaimAuditLogs.Where(a => a.ClaimId == claimId);
    }
}
