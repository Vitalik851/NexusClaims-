using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// Simulated policy entity. In production this would come from the policy-administration system;
/// here it is modelled locally so the claims module can reference policy data.
/// </summary>
public class Policy : AuditableEntity, IHasTenantIsolation
{
    public Guid PolicyId { get => Id; set => Id = value; }

    public string PolicyNumber { get; set; } = default!;

    public string ClientName { get; set; } = default!;

    public DateOnly EffectiveDate { get; set; }

    public DateOnly ExpirationDate { get; set; }

    /// <summary>
    /// Active | Expired | Cancelled
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Comma-separated list of coverage types (e.g., "Property,Liability,Auto").
    /// </summary>
    public string? CoverageTypes { get; set; }

    public Guid OrganizationEntityId { get; set; }
}
