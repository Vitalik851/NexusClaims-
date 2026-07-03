using MediatR;
using ClaimsModule.Application.DTOs.Reference;

namespace ClaimsModule.Application.Features.Reference.Queries.ListClaimStatuses;

public record ListClaimStatusesQuery : IRequest<IEnumerable<ClaimStatusTransitionDto>>;
