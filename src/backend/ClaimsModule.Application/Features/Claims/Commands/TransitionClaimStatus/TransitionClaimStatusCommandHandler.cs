using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.TransitionClaimStatus;

public class TransitionClaimStatusCommandHandler : IRequestHandler<TransitionClaimStatusCommand, Result<ClaimDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public TransitionClaimStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ClaimDetailDto>> Handle(TransitionClaimStatusCommand request, CancellationToken cancellationToken)
    {
        // 1. Load Claim with details
        var claim = await _unitOfWork.Claims.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(Claim), request.ClaimId);
        }

        var targetStatus = Enum.Parse<ClaimStatus>(request.TargetStatus);
        var oldStatus = claim.Status;

        // 2. Validate allowed transitions from transitions seed
        var allowedTransitions = await _unitOfWork.StatusTransitions.GetAllowedTransitionsAsync(claim.Status, cancellationToken);
        var matchingTransition = allowedTransitions.FirstOrDefault(t => t.ToStatus == targetStatus);

        if (matchingTransition == null)
        {
            var validNext = string.Join(", ", allowedTransitions.Select(t => t.ToStatus.ToString()));
            return Result<ClaimDetailDto>.Failure($"Transition from {claim.Status} to {targetStatus} is not permitted. Valid next statuses are: {validNext} (BR-ST-01).");
        }

        // 3. Validate user permissions for the transition
        if (!string.IsNullOrEmpty(matchingTransition.RequiredPermission))
        {
            // Simple check: manager covers supervisor, supervisor covers handler
            bool hasPermission = false;
            if (request.UserRole == "manager") hasPermission = true;
            else if (request.UserRole == "supervisor" && matchingTransition.RequiredPermission != "manager") hasPermission = true;
            else if (request.UserRole == "handler" && matchingTransition.RequiredPermission == "handler") hasPermission = true;

            if (!hasPermission)
            {
                return Result<ClaimDetailDto>.Failure($"User role '{request.UserRole}' does not have the permission '{matchingTransition.RequiredPermission}' required for this transition.");
            }
        }

        // 4. Validate Specific Business Rules per Status
        if (targetStatus == ClaimStatus.Open)
        {
            // BR-ST-02 / BR-C-03: Transitioning to Open requires at least one Claimant party
            var hasClaimant = claim.Parties.Any(p => p.PartyRole == PartyRole.Claimant && p.IsActive);
            if (!hasClaimant)
            {
                return Result<ClaimDetailDto>.Failure("Claim cannot be transitioned to Open: At least one Claimant party is required (BR-C-03).");
            }
        }
        else if (targetStatus == ClaimStatus.Closed)
        {
            // CC-01: No reserve components with PendingApproval status remain
            var hasPendingReserves = claim.ReserveComponents
                .SelectMany(r => r.History)
                .Any(h => h.ApprovalStatus == ApprovalStatus.PendingApproval);
            
            if (hasPendingReserves)
            {
                return Result<ClaimDetailDto>.Failure("Claim cannot be closed: There are pending reserve components awaiting approval (CC-01).");
            }

            // CC-03: At least one Claimant
            var hasClaimant = claim.Parties.Any(p => p.PartyRole == PartyRole.Claimant && p.IsActive);
            if (!hasClaimant)
            {
                return Result<ClaimDetailDto>.Failure("Claim cannot be closed: At least one Claimant party is required (CC-03).");
            }

            // CC-04: If any reserve component has CurrentAmount > 0, warning is returned. Requires justification note.
            var hasOpenReserves = claim.ReserveComponents.Any(r => r.Status == "Active" && r.CurrentAmount > 0);
            if (hasOpenReserves && string.IsNullOrWhiteSpace(request.Reason))
            {
                return Result<ClaimDetailDto>.Failure("Claim cannot be closed with active reserves unless a justification note is provided (CC-04).");
            }
        }
        else if (targetStatus == ClaimStatus.Reopened)
        {
            // BR-ST-04: Requires supervisor role and a non-empty reason
            if (request.UserRole != "supervisor" && request.UserRole != "manager")
            {
                return Result<ClaimDetailDto>.Failure("Supervisor or Manager role is required to reopen a claim (BR-ST-04).");
            }
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Result<ClaimDetailDto>.Failure("A reopen reason is required to reopen a claim (BR-ST-04).");
            }
        }

        // 5. Apply transition
        claim.ChangeStatus(targetStatus, request.Reason);

        // Special rule: Reopened claim immediately transitions to Open
        if (targetStatus == ClaimStatus.Reopened)
        {
            claim.ChangeStatus(ClaimStatus.Open, "Auto-opened after reopening");
        }

        if (targetStatus == ClaimStatus.Closed)
        {
            claim.ClosedAt = DateTimeOffset.UtcNow;
            claim.ClosureReason = request.Reason;
        }
        else
        {
            claim.ClosedAt = null;
            claim.ClosureReason = null;
        }

        _unitOfWork.Claims.Update(claim);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Log Audit Trail
        var eventType = targetStatus == ClaimStatus.Closed ? "CLAIM_CLOSED" : 
                        targetStatus == ClaimStatus.Reopened ? "CLAIM_REOPENED" : "STATUS_CHANGED";

        await _auditLogService.LogAsync(
            claim.ClaimId,
            eventType,
            $"Claim status transitioned from {oldStatus} to {claim.Status}. Reason: {request.Reason ?? "Not specified"}",
            oldStatus.ToString(),
            claim.Status.ToString(),
            claim.ClaimId,
            "Claim",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        var detailDto = _mapper.Map<ClaimDetailDto>(claim);
        return Result<ClaimDetailDto>.Success(detailDto);
    }
}
