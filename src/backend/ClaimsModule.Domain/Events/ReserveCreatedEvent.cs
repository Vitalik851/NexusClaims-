using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a new reserve transaction is created.
/// </summary>
public sealed record ReserveCreatedEvent(
    Guid ClaimId,
    Guid ReserveHistoryId,
    ReserveComponentType Component,
    decimal Amount,
    ApprovalStatus Status) : DomainEvent;
