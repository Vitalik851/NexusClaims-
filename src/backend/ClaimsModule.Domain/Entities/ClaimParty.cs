using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// A person or company involved in a claim (claimant, insured, witness, etc.).
/// </summary>
public class ClaimParty : AuditableEntity
{
    public Guid ClaimPartyId { get => Id; set => Id = value; }

    public Guid ClaimId { get; set; }

    public PartyRole PartyRole { get; set; }

    public PartyType PartyType { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? CompanyName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    // ──── Navigation Properties ────

    public Claim Claim { get; set; } = default!;
}
