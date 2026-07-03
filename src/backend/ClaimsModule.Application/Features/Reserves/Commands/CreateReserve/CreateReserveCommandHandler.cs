using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Commands.CreateReserve;

public class CreateReserveCommandHandler : IRequestHandler<CreateReserveCommand, Result<ReserveHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public CreateReserveCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ReserveHistoryDto>> Handle(CreateReserveCommand request, CancellationToken cancellationToken)
    {
        // 1. Load Claim with details
        var claim = await _unitOfWork.Claims.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(Claim), request.ClaimId);
        }

        // BR-C-06: Reserve creation blocked if no policy is linked
        if (!claim.PolicyId.HasValue)
        {
            return Result<ReserveHistoryDto>.Failure("Reserve creation is blocked: No policy is linked to this claim (BR-C-06).");
        }

        var componentType = Enum.Parse<ReserveComponentType>(request.Component);
        var txnType = Enum.Parse<TransactionType>(request.TransactionType);
        
        // Find or create reserve component
        var component = claim.ReserveComponents.FirstOrDefault(rc => rc.Component == componentType);
        if (component == null)
        {
            component = new ClaimReserveComponent
            {
                ClaimId = claim.ClaimId,
                Component = componentType,
                CurrentAmount = 0,
                Status = "Active"
            };
            claim.ReserveComponents.Add(component);
            await _unitOfWork.Reserves.AddComponentAsync(component, cancellationToken);
        }

        decimal previousBalance = component.CurrentAmount;
        decimal delta = request.Amount;

        // If transaction type is Adjust or Reverse, delta might modify balance differently
        if (txnType == TransactionType.Reverse)
        {
            delta = -previousBalance;
        }

        decimal proposedNewBalance = previousBalance + delta;

        // BR-R-05: Total approved reserves must not exceed $10M without manager override
        var totalApprovedReserves = claim.ReserveComponents
            .Where(rc => rc.Component != componentType && rc.Status == "Active")
            .Sum(rc => rc.CurrentAmount) + proposedNewBalance;

        if (totalApprovedReserves > 10000000 && !claim.ManagerOverrideFlag)
        {
            return Result<ReserveHistoryDto>.Failure("Total reserves will exceed $10,000,000. Manager override flag must be set on the claim first (BR-R-05).");
        }

        // BR-R-02: Check authority levels for the transaction amount (absolute value of delta)
        var absoluteAmount = Math.Abs(delta);
        var isAutoApproved = absoluteAmount <= 10000;

        var approvalStatus = isAutoApproved ? ApprovalStatus.AutoApproved : ApprovalStatus.PendingApproval;
        var postingStatus = isAutoApproved ? PostingStatus.Pending : PostingStatus.Cancelled;

        // Calculate sequence number
        var changeSeq = component.History.Any() ? component.History.Max(h => h.ChangeSequence) + 1 : 1;

        var historyId = Guid.NewGuid();
        var history = new ReserveHistory
        {
            ReserveHistoryId = historyId,
            ReserveComponentId = component.ReserveComponentId,
            ClaimId = claim.ClaimId,
            TransactionType = txnType,
            Amount = delta,
            PreviousBalance = previousBalance,
            NewBalance = isAutoApproved ? proposedNewBalance : previousBalance, // Update balance only if approved
            ApprovalStatus = approvalStatus,
            ChangeReason = request.ChangeReason,
            PostingStatus = postingStatus,
            IdempotencyKey = $"Reserve:{component.ReserveComponentId}:Change:{changeSeq}",
            ChangeSequence = changeSeq,
            SubmittedByUserId = request.CurrentUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (isAutoApproved)
        {
            component.CurrentAmount = proposedNewBalance;
        }

        component.History.Add(history);
        _unitOfWork.Reserves.UpdateComponent(component);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add domain event
        claim.AddDomainEvent(new ReserveCreatedEvent(claim.ClaimId, history.ReserveHistoryId, componentType, delta, approvalStatus));

        // Audit Logging
        await _auditLogService.LogAsync(
            claim.ClaimId,
            "RESERVE_CREATED",
            $"Reserve transaction of {delta:C} ({txnType}) created for component {componentType}.",
            previousBalance.ToString("F4"),
            proposedNewBalance.ToString("F4"),
            history.ReserveHistoryId,
            "ReserveHistory",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        if (isAutoApproved)
        {
            await _auditLogService.LogAsync(
                claim.ClaimId,
                "RESERVE_AUTO_APPROVED",
                $"Reserve component {componentType} was automatically approved (amount {absoluteAmount:C} <= $10,000).",
                null,
                proposedNewBalance.ToString("F4"),
                history.ReserveHistoryId,
                "ReserveHistory",
                request.CurrentUserId,
                request.CorrelationId,
                cancellationToken);
        }

        var historyDto = _mapper.Map<ReserveHistoryDto>(history);
        return Result<ReserveHistoryDto>.Success(historyDto);
    }
}
