namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Generates unique, tenant-scoped claim numbers.
/// </summary>
public interface IClaimNumberGenerator
{
    /// <summary>
    /// Generates the next unique claim number for the given organisation.
    /// </summary>
    /// <param name="organizationId">The tenant / organisation entity identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A unique claim number string (e.g., "CLM-2026-000042").</returns>
    Task<string> GenerateAsync(Guid organizationId, CancellationToken ct = default);
}
