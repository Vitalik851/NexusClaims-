using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Repository for querying the (simulated) policy store.
/// </summary>
public interface IPolicyRepository
{
    /// <summary>
    /// Retrieves a policy by its identifier.
    /// </summary>
    Task<Policy?> GetByIdAsync(Guid policyId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a policy by its identifier, including parsed coverage information.
    /// </summary>
    Task<Policy?> GetByIdWithCoverageAsync(Guid policyId, CancellationToken ct = default);

    /// <summary>
    /// Searches policies by a free-text query against policy number and client name.
    /// </summary>
    /// <param name="query">Search term.</param>
    /// <param name="organizationId">Tenant scope.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Matching policies, limited to a reasonable result set.</returns>
    Task<List<Policy>> SearchAsync(string query, Guid organizationId, CancellationToken ct = default);
}
