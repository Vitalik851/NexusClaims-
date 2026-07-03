using MediatR;
using ClaimsModule.Application.DTOs.Documents;

namespace ClaimsModule.Application.Features.Documents.Queries.ListDocuments;

public record ListDocumentsQuery(Guid ClaimId) : IRequest<IEnumerable<ClaimDocumentDto>>;
