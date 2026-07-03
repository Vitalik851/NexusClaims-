using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Queries.ListClaims;

public class ListClaimsQueryHandler : IRequestHandler<ListClaimsQuery, PaginatedList<ClaimSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListClaimsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ClaimSummaryDto>> Handle(ListClaimsQuery request, CancellationToken cancellationToken)
    {
        ClaimStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ClaimStatus>(request.Status, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var orgId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var (items, totalCount) = await _unitOfWork.Claims.ListAsync(
            orgId,
            statusEnum,
            request.DateFrom,
            request.DateTo,
            request.AssignedHandlerId,
            request.CauseOfLossCode,
            request.PolicyId,
            request.Search,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = _mapper.Map<List<ClaimSummaryDto>>(items);

        return PaginatedList<ClaimSummaryDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
