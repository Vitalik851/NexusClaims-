using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Commands.RejectReserve;

public class RejectReserveCommandHandler : IRequestHandler<RejectReserveCommand, Result<ReserveHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public RejectReserveCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ReserveHistoryDto>> Handle(RejectReserveCommand request, CancellationToken cancellationToken)
    {
        // 1. Load pending transaction
        var history = await _unitOfWork.Reserves.GetPendingByIdAsync(request.ReserveHistoryId, cancellationToken);
        if (history == null)
        {
            throw new NotFoundException(nameof(ReserveHistory), request.ReserveHistoryId);
        }

        if (history.ApprovalStatus != ApprovalStatus.PendingApproval)
        {
            return Result<ReserveHistoryDto>.Failure("This reserve transaction is not in PendingApproval status.");
        }

        // Validate user role: requires supervisor or manager
        if (request.UserRole != "supervisor" && request.UserRole != "manager")
        {
            return Result<ReserveHistoryDto>.Failure("Supervisor or Manager role is required to reject a reserve component.");
        }

        // 2. Reject transaction
        history.ApprovalStatus = ApprovalStatus.Rejected;
        history.RejectedByUserId = request.CurrentUserId;
        history.RejectedAt = DateTimeOffset.UtcNow;
        history.RejectionReason = request.RejectionReason;
        history.PostingStatus = PostingStatus.Cancelled;

        _unitOfWork.Reserves.UpdateHistory(history);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Raise ReserveRejectedEvent
        var claim = await _unitOfWork.Claims.GetByIdAsync(history.ClaimId, cancellationToken);
        claim?.AddDomainEvent(new ReserveRejectedEvent(history.ClaimId, history.ReserveHistoryId, request.CurrentUserId, request.RejectionReason));

        // 3. Audit Logging
        await _auditLogService.LogAsync(
            history.ClaimId,
            "RESERVE_REJECTED",
            $"Reserve transaction of {history.Amount:C} for component {history.ReserveComponent.Component} was rejected by {request.UserRole}. Reason: {request.RejectionReason}",
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
