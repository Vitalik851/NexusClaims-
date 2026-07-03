namespace ClaimsModule.Application.DTOs.Documents;

public class ClaimDocumentDto
{
    public Guid ClaimDocumentId { get; set; }
    public Guid ClaimId { get; set; }
    public string DocumentType { get; set; } = null!;
    public string DocumentName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public Guid? UploadedByUserId { get; set; }
    public string? Notes { get; set; }
    public string? DownloadUrl { get; set; }
}
