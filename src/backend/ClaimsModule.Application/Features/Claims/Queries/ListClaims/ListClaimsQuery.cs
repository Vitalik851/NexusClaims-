using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Queries.ListClaims;

public record ListClaimsQuery : IRequest<PaginatedList<ClaimSummaryDto>>
{
    public string? Status { get; init; }
    public DateTimeOffset? DateFrom { get; init; }
    public DateTimeOffset? DateTo { get; init; }
    public Guid? AssignedHandlerId { get; init; }
    public string? CauseOfLossCode { get; init; }
    public Guid? PolicyId { get; init; }
    public string? Search { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
