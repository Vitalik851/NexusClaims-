using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// Aggregate root representing an insurance claim.
/// Owns loss events, parties, risk objects, reserves, documents, and audit logs.
/// </summary>
public class Claim : AuditableEntity, IHasTenantIsolation
{
    public Guid ClaimId { get => Id; set => Id = value; }

    public Guid OrganizationEntityId { get; set; }

    public string ClaimNumber { get; set; } = default!;

    public Guid? PolicyId { get; set; }

    public string? PolicyNumber { get; set; }

    public string? ClientName { get; set; }

    public ClaimStatus Status { get; set; }

    public ClaimSeverity? Severity { get; set; }

    public DateTimeOffset ReportedDate { get; set; }

    public Guid? AssignedHandlerId { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public string? ClosureReason { get; set; }

    public string? Notes { get; set; }

    public bool ManagerOverrideFlag { get; set; }

    /// <summary>
    /// Optimistic concurrency token.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    // ──── Navigation Properties ────

    public LossEvent? LossEvent { get; set; }

    public List<ClaimParty> Parties { get; set; } = [];

    public List<ClaimRiskObject> RiskObjects { get; set; } = [];

    public List<ClaimReserveComponent> ReserveComponents { get; set; } = [];

    public List<ClaimDocument> Documents { get; set; } = [];

    public List<ClaimAuditLog> AuditLogs { get; set; } = [];

    // ──── Domain Behaviour ────

    public void ChangeStatus(ClaimStatus newStatus, string? reason = null)
    {
        var oldStatus = Status;
        Status = newStatus;

        if (newStatus == ClaimStatus.Closed)
        {
            ClosedAt = DateTimeOffset.UtcNow;
            ClosureReason = reason;
        }

        AddDomainEvent(new ClaimStatusChangedEvent(Id, oldStatus, newStatus, reason));
    }

    public ClaimParty AddParty(PartyRole role, PartyType type, string? firstName, string? lastName,
        string? companyName, string? email, string? phone, string? notes)
    {
        var party = new ClaimParty
        {
            Id = Guid.NewGuid(),
            ClaimId = Id,
            PartyRole = role,
            PartyType = type,
            FirstName = firstName,
            LastName = lastName,
            CompanyName = companyName,
            Email = email,
            Phone = phone,
            Notes = notes,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Parties.Add(party);
        AddDomainEvent(new PartyAddedEvent(Id, party.Id, role));
        return party;
    }

    public void RemoveParty(Guid partyId)
    {
        var party = Parties.Find(p => p.Id == partyId);
        if (party is not null)
        {
            party.IsActive = false;
            AddDomainEvent(new PartyRemovedEvent(Id, partyId));
        }
    }
}
