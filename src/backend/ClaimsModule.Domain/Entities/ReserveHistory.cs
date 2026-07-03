using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Enumerations;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// An immutable audit record of a single reserve change transaction.
/// Tracks amounts, approval workflow, and posting status.
/// </summary>
public class ReserveHistory : BaseEntity
{
    public Guid ReserveHistoryId { get => Id; set => Id = value; }

    public Guid ReserveComponentId { get; set; }

    public Guid ClaimId { get; set; }

    public TransactionType TransactionType { get; set; }

    public decimal Amount { get; set; }

    public decimal PreviousBalance { get; set; }

    public decimal NewBalance { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid? RejectedByUserId { get; set; }

    public DateTimeOffset? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }

    public string? ChangeReason { get; set; }

    public PostingStatus PostingStatus { get; set; }

    public Guid? PostingJobId { get; set; }

    public string IdempotencyKey { get; set; } = null!;

    public int ChangeSequence { get; set; }

    public Guid? SubmittedByUserId { get; set; }

    // ──── Navigation Properties ────

    public ClaimReserveComponent ReserveComponent { get; set; } = default!;
}
