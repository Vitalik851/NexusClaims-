using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Audit;

namespace ClaimsModule.Application.Features.Claims.Queries.GetClaimAuditLog;

public record GetClaimAuditLogQuery : IRequest<PaginatedList<AuditLogEntryDto>>
{
    public Guid ClaimId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public GetClaimAuditLogQuery(Guid claimId, int pageNumber = 1, int pageSize = 20)
    {
        ClaimId = claimId;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
