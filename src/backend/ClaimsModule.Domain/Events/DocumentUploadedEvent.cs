namespace ClaimsModule.Domain.Events;

/// <summary>
/// Raised when a document is uploaded to a claim.
/// </summary>
public sealed record DocumentUploadedEvent(
    Guid ClaimId,
    Guid DocumentId,
    string FileName) : DomainEvent;
