using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Repository for reserve components and their history.
/// </summary>
public interface IReserveRepository
{
    /// <summary>
    /// Retrieves all reserve components for a claim.
    /// </summary>
    Task<List<ClaimReserveComponent>> GetByClaimIdAsync(Guid claimId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the full reserve change history for a claim, ordered by sequence.
    /// </summary>
    Task<List<ReserveHistory>> GetHistoryByClaimIdAsync(Guid claimId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single reserve history entry that is in a pending-approval state.
    /// </summary>
    Task<ReserveHistory?> GetPendingByIdAsync(Guid reserveHistoryId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new reserve component.
    /// </summary>
    Task AddComponentAsync(ClaimReserveComponent component, CancellationToken ct = default);

    /// <summary>
    /// Adds a new reserve history entry.
    /// </summary>
    Task AddHistoryAsync(ReserveHistory history, CancellationToken ct = default);

    /// <summary>
    /// Marks a reserve component as modified.
    /// </summary>
    void UpdateComponent(ClaimReserveComponent component);

    /// <summary>
    /// Marks a reserve history entry as modified.
    /// </summary>
    void UpdateHistory(ReserveHistory history);
}
