using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Repository for the <see cref="Claim"/> aggregate root.
/// </summary>
public interface IClaimRepository
{
    /// <summary>
    /// Retrieves a claim by its identifier, returning <c>null</c> if not found.
    /// </summary>
    Task<Claim?> GetByIdAsync(Guid claimId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a claim by its identifier including all child entities
    /// (loss event, parties, risk objects, reserve components, documents).
    /// </summary>
    Task<Claim?> GetByIdWithDetailsAsync(Guid claimId, CancellationToken ct = default);

    /// <summary>
    /// Returns a paged list of claims, optionally filtered by organisation, status, handler,
    /// and a free-text search term.
    /// </summary>
    /// <param name="organizationId">Tenant filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="assignedHandlerId">Optional handler filter.</param>
    /// <param name="searchTerm">Optional free-text search over claim number and client name.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple of the matching claims and the total count (for paging).</returns>
    Task<(List<Claim> Items, int TotalCount)> ListAsync(
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
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new claim to the repository.
    /// </summary>
    Task AddAsync(Claim claim, CancellationToken ct = default);

    /// <summary>
    /// Marks a claim as modified in the change tracker.
    /// </summary>
    void Update(Claim claim);

    /// <summary>
    /// Soft-deletes a claim.
    /// </summary>
    void Delete(Claim claim);
}
