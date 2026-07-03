namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Service responsible for persisting claim-level audit log entries.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Records an audit log entry against a claim.
    /// </summary>
    /// <param name="claimId">The claim this event relates to.</param>
    /// <param name="eventType">Short classifier for the event (e.g., "StatusChanged", "ReserveAdjusted").</param>
    /// <param name="description">Human-readable description of what happened.</param>
    /// <param name="oldValue">Serialised previous state, if applicable.</param>
    /// <param name="newValue">Serialised new state, if applicable.</param>
    /// <param name="relatedEntityId">Optional FK to the entity that changed.</param>
    /// <param name="relatedEntityType">Type name of the related entity.</param>
    /// <param name="userId">The user who triggered the event.</param>
    /// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
    /// <param name="ct">Cancellation token.</param>
    Task LogAsync(
        Guid claimId,
        string eventType,
        string description,
        string? oldValue,
        string? newValue,
        Guid? relatedEntityId,
        string? relatedEntityType,
        Guid? userId,
        Guid? correlationId,
        CancellationToken ct = default);
}
