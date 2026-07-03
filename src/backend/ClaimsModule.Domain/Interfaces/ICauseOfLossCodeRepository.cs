using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Repository for cause-of-loss lookup codes.
/// </summary>
public interface ICauseOfLossCodeRepository
{
    /// <summary>
    /// Returns all active cause-of-loss codes, ordered by <see cref="CauseOfLossCode.SortOrder"/>.
    /// </summary>
    Task<List<CauseOfLossCode>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a single cause-of-loss code by its unique short code.
    /// </summary>
    Task<CauseOfLossCode?> GetByCodeAsync(string code, CancellationToken ct = default);
}
