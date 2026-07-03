namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a new claim is created and persisted.
/// </summary>
public sealed record ClaimCreatedEvent(Guid ClaimId, string ClaimNumber) : DomainEvent;
