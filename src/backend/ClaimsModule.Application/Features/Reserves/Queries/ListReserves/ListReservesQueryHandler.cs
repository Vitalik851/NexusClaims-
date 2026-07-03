using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Queries.ListReserves;

public class ListReservesQueryHandler : IRequestHandler<ListReservesQuery, ReserveSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListReservesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ReserveSummaryDto> Handle(ListReservesQuery request, CancellationToken cancellationToken)
    {
        var components = await _unitOfWork.Reserves.GetByClaimIdAsync(request.ClaimId, cancellationToken);
        var history = await _unitOfWork.Reserves.GetHistoryByClaimIdAsync(request.ClaimId, cancellationToken);

        return new ReserveSummaryDto
        {
            Components = _mapper.Map<List<ReserveComponentSummaryDto>>(components),
            History = _mapper.Map<List<ReserveHistoryDto>>(history)
        };
    }
}
