namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a pending reserve change is approved.
/// </summary>
public sealed record ReserveApprovedEvent(
    Guid ClaimId,
    Guid ReserveHistoryId,
    Guid ApprovedByUserId) : DomainEvent;
