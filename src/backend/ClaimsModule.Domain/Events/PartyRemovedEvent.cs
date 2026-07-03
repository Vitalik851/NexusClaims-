namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a party is removed (deactivated) from a claim.
/// </summary>
public sealed record PartyRemovedEvent(
    Guid ClaimId,
    Guid PartyId) : DomainEvent;
