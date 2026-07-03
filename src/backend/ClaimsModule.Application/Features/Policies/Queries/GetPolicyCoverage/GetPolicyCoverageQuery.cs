using MediatR;
using ClaimsModule.Application.DTOs.Policies;

namespace ClaimsModule.Application.Features.Policies.Queries.GetPolicyCoverage;

public record GetPolicyCoverageQuery(Guid PolicyId) : IRequest<IEnumerable<PolicyCoverageDto>>;
