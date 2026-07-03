namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a pending reserve change is rejected.
/// </summary>
public sealed record ReserveRejectedEvent(
    Guid ClaimId,
    Guid ReserveHistoryId,
    Guid RejectedByUserId,
    string Reason) : DomainEvent;
