using MediatR;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Policies;

namespace ClaimsModule.Application.Features.Policies.Queries.GetPolicyCoverage;

public class GetPolicyCoverageQueryHandler : IRequestHandler<GetPolicyCoverageQuery, IEnumerable<PolicyCoverageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPolicyCoverageQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PolicyCoverageDto>> Handle(GetPolicyCoverageQuery request, CancellationToken cancellationToken)
    {
        var policy = await _unitOfWork.Policies.GetByIdAsync(request.PolicyId, cancellationToken);
        if (policy == null)
        {
            throw new NotFoundException(nameof(Policy), request.PolicyId);
        }

        var types = (policy.CoverageTypes ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var result = new List<PolicyCoverageDto>();

        foreach (var type in types)
        {
            var trimmed = type.Trim();
            result.Add(new PolicyCoverageDto
            {
                CoverageType = trimmed,
                Description = $"{trimmed} protection coverage under policy {policy.PolicyNumber}"
            });
        }

        return result;
    }
}
