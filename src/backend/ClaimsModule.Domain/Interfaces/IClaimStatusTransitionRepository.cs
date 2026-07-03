using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Repository for querying valid claim status transitions.
/// </summary>
public interface IClaimStatusTransitionRepository
{
    /// <summary>
    /// Returns all transitions allowed from the specified status.
    /// </summary>
    /// <param name="fromStatus">The current claim status.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Valid target statuses and their required permissions.</returns>
    Task<List<ClaimStatusTransition>> GetAllowedTransitionsAsync(
        ClaimStatus fromStatus,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all transitions configured in the system.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<List<ClaimStatusTransition>> GetAllTransitionsAsync(CancellationToken ct = default);
}
