using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Repository interface for ClaimAuditLog entity.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets a queryable source of audit log entries for a specific claim.
    /// </summary>
    IQueryable<ClaimAuditLog> GetQueryableByClaimId(Guid claimId);
}
