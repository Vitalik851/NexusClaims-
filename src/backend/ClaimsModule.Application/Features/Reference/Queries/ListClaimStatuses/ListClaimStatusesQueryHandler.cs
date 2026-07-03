using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.DTOs.Reference;

namespace ClaimsModule.Application.Features.Reference.Queries.ListClaimStatuses;

public class ListClaimStatusesQueryHandler : IRequestHandler<ListClaimStatusesQuery, IEnumerable<ClaimStatusTransitionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListClaimStatusesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ClaimStatusTransitionDto>> Handle(ListClaimStatusesQuery request, CancellationToken cancellationToken)
    {
        var transitions = await _unitOfWork.StatusTransitions.GetAllTransitionsAsync(cancellationToken);
        return _mapper.Map<List<ClaimStatusTransitionDto>>(transitions);
    }
}
