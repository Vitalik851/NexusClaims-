using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// Metadata for a document attached to a claim. The binary content resides in blob storage.
/// </summary>
public class ClaimDocument : AuditableEntity
{
    public Guid ClaimDocumentId { get => Id; set => Id = value; }

    public Guid ClaimId { get; set; }

    public string DocumentType { get; set; } = default!;

    public string DocumentName { get; set; } = default!;

    public string BlobPath { get; set; } = default!;

    public string ContentType { get; set; } = default!;

    public long FileSizeBytes { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public Guid? UploadedByUserId { get; set; }

    public string? Notes { get; set; }

    // ──── Navigation Properties ────

    public Claim Claim { get; set; } = default!;
}
