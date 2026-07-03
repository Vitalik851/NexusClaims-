using MediatR;

namespace ClaimsModule.Domain.Events;

/// <summary>
/// Base class for all domain events raised by aggregate roots.
/// Each event carries a unique identifier and the timestamp at which it occurred.
/// </summary>
public abstract record DomainEvent : INotification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
