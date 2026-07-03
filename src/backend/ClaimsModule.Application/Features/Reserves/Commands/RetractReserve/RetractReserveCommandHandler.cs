using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Commands.RetractReserve;

public class RetractReserveCommandHandler : IRequestHandler<RetractReserveCommand, Result<ReserveHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public RetractReserveCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ReserveHistoryDto>> Handle(RetractReserveCommand request, CancellationToken cancellationToken)
    {
        // 1. Load pending transaction
        var history = await _unitOfWork.Reserves.GetPendingByIdAsync(request.ReserveHistoryId, cancellationToken);
        if (history == null)
        {
            throw new NotFoundException(nameof(ReserveHistory), request.ReserveHistoryId);
        }

        if (history.ApprovalStatus != ApprovalStatus.PendingApproval)
        {
            return Result<ReserveHistoryDto>.Failure("Only pending reserve transactions can be retracted.");
        }

        // Verify submitter: only the user who created it can retract it
        if (history.SubmittedByUserId.HasValue && history.SubmittedByUserId.Value != request.CurrentUserId)
        {
            return Result<ReserveHistoryDto>.Failure("You can only retract your own reserve transactions.");
        }

        // 2. Retract transaction
        history.ApprovalStatus = ApprovalStatus.Cancelled;
        history.PostingStatus = PostingStatus.Cancelled;

        _unitOfWork.Reserves.UpdateHistory(history);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Audit Logging
        await _auditLogService.LogAsync(
            history.ClaimId,
            "RESERVE_RETRACTED",
            $"Reserve transaction of {history.Amount:C} for component {history.ReserveComponent.Component} was retracted by the submitter.",
            null,
            null,
            history.ReserveHistoryId,
            "ReserveHistory",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        var historyDto = _mapper.Map<ReserveHistoryDto>(history);
        return Result<ReserveHistoryDto>.Success(historyDto);
    }
}
