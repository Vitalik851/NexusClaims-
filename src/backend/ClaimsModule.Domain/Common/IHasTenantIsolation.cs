namespace ClaimsModule.Domain.Common;

/// <summary>
/// Marker interface for entities that are scoped to a specific organisation/tenant.
/// </summary>
public interface IHasTenantIsolation
{
    Guid OrganizationEntityId { get; }
}
