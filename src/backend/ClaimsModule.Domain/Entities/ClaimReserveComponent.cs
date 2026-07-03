using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// A financial reserve component (e.g., Indemnity, Expense) on a claim.
/// Tracks the current outstanding amount and owns a history of reserve changes.
/// </summary>
public class ClaimReserveComponent : AuditableEntity
{
    public Guid ReserveComponentId { get => Id; set => Id = value; }

    public Guid ClaimId { get; set; }

    public ReserveComponentType Component { get; set; }

    public decimal CurrentAmount { get; set; }

    public string Status { get; set; } = "Active";

    public string? Notes { get; set; }

    /// <summary>
    /// Optimistic concurrency token.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    // ──── Navigation Properties ────

    public Claim Claim { get; set; } = default!;

    public List<ReserveHistory> History { get; set; } = [];
}
