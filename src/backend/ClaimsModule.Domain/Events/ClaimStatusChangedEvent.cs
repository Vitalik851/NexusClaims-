using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a claim transitions from one status to another.
/// </summary>
public sealed record ClaimStatusChangedEvent(
    Guid ClaimId,
    ClaimStatus OldStatus,
    ClaimStatus NewStatus,
    string? Reason) : DomainEvent;
