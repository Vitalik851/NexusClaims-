using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a party is added to a claim.
/// </summary>
public sealed record PartyAddedEvent(
    Guid ClaimId,
    Guid PartyId,
    PartyRole Role) : DomainEvent;
