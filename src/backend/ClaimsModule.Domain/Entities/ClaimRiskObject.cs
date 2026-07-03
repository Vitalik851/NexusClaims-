using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// An insured asset or risk object associated with a claim.
/// </summary>
public class ClaimRiskObject : AuditableEntity
{
    public Guid ClaimRiskObjectId { get => Id; set => Id = value; }

    public Guid ClaimId { get; set; }

    public AssetType AssetType { get; set; }

    public string AssetDescription { get; set; } = default!;

    public string? DamageDescription { get; set; }

    public bool IsPrimary { get; set; }

    public string? AssetReference { get; set; }

    // ──── Navigation Properties ────

    public Claim Claim { get; set; } = default!;
}
