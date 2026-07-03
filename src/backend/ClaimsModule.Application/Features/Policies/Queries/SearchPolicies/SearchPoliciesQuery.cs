using MediatR;
using ClaimsModule.Application.DTOs.Policies;

namespace ClaimsModule.Application.Features.Policies.Queries.SearchPolicies;

public record SearchPoliciesQuery(string? Query) : IRequest<IEnumerable<PolicySearchResultDto>>;
