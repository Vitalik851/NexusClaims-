using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Commands.ApproveReserve;

public class ApproveReserveCommandHandler : IRequestHandler<ApproveReserveCommand, Result<ReserveHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public ApproveReserveCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ReserveHistoryDto>> Handle(ApproveReserveCommand request, CancellationToken cancellationToken)
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

        // BR-R-03: Submitter cannot approve their own transaction (Self-approval is not permitted)
        if (history.SubmittedByUserId.HasValue && history.SubmittedByUserId.Value == request.CurrentUserId)
        {
            return Result<ReserveHistoryDto>.Failure("Self-approval is not permitted (BR-R-03).");
        }

        // BR-R-02: Validate approver role based on transaction amount
        var absoluteAmount = Math.Abs(history.Amount);
        
        bool isAuthorized = false;
        if (request.UserRole == "manager")
        {
            isAuthorized = true;
        }
        else if (request.UserRole == "supervisor" && absoluteAmount <= 100000)
        {
            isAuthorized = true;
        }

        if (!isAuthorized)
        {
            return Result<ReserveHistoryDto>.Failure($"Your role '{request.UserRole}' does not have authority to approve this reserve amount (amount: {absoluteAmount:C}) (BR-R-02).");
        }

        // 2. Approve transaction and apply balance update
        var component = history.ReserveComponent;
        decimal previousBalance = component.CurrentAmount;
        decimal newBalance = previousBalance + history.Amount;

        history.ApprovalStatus = ApprovalStatus.Approved;
        history.ApprovedByUserId = request.CurrentUserId;
        history.ApprovedAt = DateTimeOffset.UtcNow;
        history.PreviousBalance = previousBalance;
        history.NewBalance = newBalance;
        history.PostingStatus = PostingStatus.Pending; // Will trigger GL job

        component.CurrentAmount = newBalance;

        _unitOfWork.Reserves.UpdateHistory(history);
        _unitOfWork.Reserves.UpdateComponent(component);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Raise ReserveApprovedEvent to trigger Hangfire GL Job
        // The event handler in Infrastructure will schedule it
        var claim = await _unitOfWork.Claims.GetByIdAsync(history.ClaimId, cancellationToken);
        claim?.AddDomainEvent(new ReserveApprovedEvent(history.ClaimId, history.ReserveHistoryId, request.CurrentUserId));

        // 3. Log Audit logs
        await _auditLogService.LogAsync(
            history.ClaimId,
            "RESERVE_APPROVED",
            $"Reserve transaction of {history.Amount:C} for component {component.Component} was approved by {request.UserRole}.",
            previousBalance.ToString("F4"),
            newBalance.ToString("F4"),
            history.ReserveHistoryId,
            "ReserveHistory",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        var historyDto = _mapper.Map<ReserveHistoryDto>(history);
        return Result<ReserveHistoryDto>.Success(historyDto);
    }
}
