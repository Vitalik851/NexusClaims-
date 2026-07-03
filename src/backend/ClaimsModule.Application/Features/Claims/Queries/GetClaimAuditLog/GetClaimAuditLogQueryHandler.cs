using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Audit;

namespace ClaimsModule.Application.Features.Claims.Queries.GetClaimAuditLog;

public class GetClaimAuditLogQueryHandler : IRequestHandler<GetClaimAuditLogQuery, PaginatedList<AuditLogEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetClaimAuditLogQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AuditLogEntryDto>> Handle(GetClaimAuditLogQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.AuditLogs.GetQueryableByClaimId(request.ClaimId)
            .OrderByDescending(a => a.CreatedAt);

        var paginatedList = await PaginatedList<ClaimsModule.Domain.Entities.ClaimAuditLog>.CreateAsync(
            query, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = _mapper.Map<List<AuditLogEntryDto>>(paginatedList.Items);

        return new PaginatedList<AuditLogEntryDto>(dtos, paginatedList.TotalCount, request.PageNumber, request.PageSize);
    }
}
