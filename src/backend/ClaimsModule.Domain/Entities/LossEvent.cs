using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// Describes the loss incident that gave rise to a claim.
/// One-to-one relationship with <see cref="Claim"/>.
/// </summary>
public class LossEvent : AuditableEntity
{
    public Guid LossEventId { get => Id; set => Id = value; }

    public Guid ClaimId { get; set; }

    public DateTimeOffset LossDate { get; set; }

    public string LossDescription { get; set; } = default!;

    public string? LossLocation { get; set; }

    public string? CauseOfLossCode { get; set; }

    public decimal? EstimatedLossAmount { get; set; }

    public DateTimeOffset? ReportDate { get; set; }

    public string? PoliceReportNumber { get; set; }

    // ──── Navigation Properties ────

    public Claim Claim { get; set; } = default!;

    public CauseOfLossCode? CauseOfLossCodeNavigation { get; set; }
}
