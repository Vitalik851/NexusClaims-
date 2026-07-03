using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// Immutable audit log entry recording a significant event on a claim.
/// </summary>
public class ClaimAuditLog : BaseEntity
{
    public Guid AuditLogId { get => Id; set => Id = value; }

    public Guid ClaimId { get; set; }

    public string EventType { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }

    public Guid? CorrelationId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    // ──── Navigation Properties ────

    public Claim Claim { get; set; } = default!;
}
