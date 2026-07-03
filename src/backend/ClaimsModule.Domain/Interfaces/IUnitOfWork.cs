namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Unit of Work exposing all domain repositories and a single <see cref="SaveChangesAsync"/> commit point.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Claim aggregate repository.</summary>
    IClaimRepository Claims { get; }

    /// <summary>Reserve component and history repository.</summary>
    IReserveRepository Reserves { get; }

    /// <summary>Policy repository (simulated).</summary>
    IPolicyRepository Policies { get; }

    /// <summary>Cause-of-loss lookup code repository.</summary>
    ICauseOfLossCodeRepository CauseOfLossCodes { get; }

    /// <summary>Claim status transition rules repository.</summary>
    IClaimStatusTransitionRepository StatusTransitions { get; }

    /// <summary>Audit log entries repository.</summary>
    IAuditLogRepository AuditLogs { get; }

    /// <summary>
    /// Commits all pending changes across repositories in a single database transaction.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
