using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.DTOs.Reference;

namespace ClaimsModule.Application.Features.Reference.Queries.ListCauseOfLossCodes;

public class ListCauseOfLossCodesQueryHandler : IRequestHandler<ListCauseOfLossCodesQuery, IEnumerable<CauseOfLossCodeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListCauseOfLossCodesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CauseOfLossCodeDto>> Handle(ListCauseOfLossCodesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<CauseOfLossCode> codes = await _unitOfWork.CauseOfLossCodes.GetAllActiveAsync(cancellationToken);
        
        if (!string.IsNullOrEmpty(request.PerilCategory))
        {
            codes = codes.Where(c => c.PerilCategory.Equals(request.PerilCategory, StringComparison.OrdinalIgnoreCase));
        }

        return _mapper.Map<List<CauseOfLossCodeDto>>(codes);
    }
}
