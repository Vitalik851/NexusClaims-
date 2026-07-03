using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Documents;

namespace ClaimsModule.Application.Features.Documents.Commands.UploadDocument;

public record UploadDocumentCommand : IRequest<Result<ClaimDocumentDto>>
{
    public Guid ClaimId { get; init; }
    public string DocumentType { get; init; } = null!;
    public string DocumentName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public long FileSizeBytes { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string? Notes { get; init; }

    // User context
    public Guid CurrentUserId { get; init; }
    public Guid? CorrelationId { get; init; }
}
