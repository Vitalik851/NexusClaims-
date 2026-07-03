using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.DTOs.Policies;

namespace ClaimsModule.Application.Features.Policies.Queries.SearchPolicies;

public class SearchPoliciesQueryHandler : IRequestHandler<SearchPoliciesQuery, IEnumerable<PolicySearchResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchPoliciesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PolicySearchResultDto>> Handle(SearchPoliciesQuery request, CancellationToken cancellationToken)
    {
        var orgId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var policies = await _unitOfWork.Policies.SearchAsync(request.Query ?? string.Empty, orgId, cancellationToken);
        return _mapper.Map<List<PolicySearchResultDto>>(policies);
    }
}
