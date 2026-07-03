using ClaimsModule.Domain.Events;

namespace ClaimsModule.Domain.Common;

/// <summary>
/// Base entity providing identity, audit timestamps, and domain event support.
/// All entities in the domain inherit from this class.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UserCreated { get; set; }

    public Guid? UserModified { get; set; }

    private readonly List<DomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this entity.
    /// Not persisted — consumed and cleared after unit-of-work commit.
    /// </summary>
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
