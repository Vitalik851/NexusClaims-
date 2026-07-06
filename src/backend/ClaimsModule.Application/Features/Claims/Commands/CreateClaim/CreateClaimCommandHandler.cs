using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.CreateClaim;

public class CreateClaimCommandHandler : IRequestHandler<CreateClaimCommand, Result<ClaimDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClaimNumberGenerator _claimNumberGenerator;
    private readonly IAuditLogService _auditLogService;

    public CreateClaimCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IClaimNumberGenerator claimNumberGenerator,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _claimNumberGenerator = claimNumberGenerator;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ClaimDetailDto>> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        var warnings = new List<string>();

        // 1. Validate Cause of Loss Code
        var causeOfLoss = await _unitOfWork.CauseOfLossCodes.GetByCodeAsync(request.CauseOfLossCode, cancellationToken);
        if (causeOfLoss == null || !causeOfLoss.IsActive)
        {
            return Result<ClaimDetailDto>.Failure("Cause of loss code is not recognized or is inactive (BR-C-05).");
        }

        // 2. Policy Lookup and validation
        Policy? policy = null;
        if (request.PolicyId.HasValue)
        {
            policy = await _unitOfWork.Policies.GetByIdAsync(request.PolicyId.Value, cancellationToken);
            if (policy == null)
            {
                warnings.Add("Policy not found.");
            }
            else
            {
                // BR-C-02: Loss date within effective period validation
                var lossDateOnly = DateOnly.FromDateTime(request.LossDate.DateTime);
                if (lossDateOnly < policy.EffectiveDate || lossDateOnly > policy.ExpirationDate)
                {
                    warnings.Add("Loss date is outside the policy effective period.");
                }
            }
        }
        else
        {
            warnings.Add("No policy linked — claim requires policy association before financial actions are permitted.");
        }

        // 3. Generate atomic Claim Number
        var claimNumber = await _claimNumberGenerator.GenerateAsync(request.OrganizationEntityId, cancellationToken);

        // 4. Create Claim Aggregate Root
        var claim = new Claim
        {
            Id = Guid.NewGuid(),
            OrganizationEntityId = request.OrganizationEntityId,
            ClaimNumber = claimNumber,
            PolicyId = request.PolicyId,
            PolicyNumber = policy?.PolicyNumber,
            ClientName = policy?.ClientName,
            Status = ClaimStatus.Draft,
            Severity = !string.IsNullOrEmpty(request.Severity) && Enum.TryParse<ClaimSeverity>(request.Severity, out var sev) ? sev : ClaimSeverity.Standard,
            ReportedDate = DateTimeOffset.UtcNow,
            Notes = request.Notes,
            ManagerOverrideFlag = false
        };

        // 5. Create Loss Event
        claim.LossEvent = new LossEvent
        {
            LossDate = request.LossDate,
            LossDescription = request.LossDescription,
            LossLocation = request.LossLocation,
            CauseOfLossCode = request.CauseOfLossCode,
            EstimatedLossAmount = request.EstimatedLossAmount,
            ReportDate = DateTimeOffset.UtcNow
        };

        // 6. Add Parties
        foreach (var p in request.Parties)
        {
            claim.Parties.Add(new ClaimParty
            {
                PartyRole = Enum.Parse<PartyRole>(p.PartyRole),
                PartyType = Enum.Parse<PartyType>(p.PartyType),
                FirstName = p.FirstName,
                LastName = p.LastName,
                CompanyName = p.CompanyName,
                Email = p.Email,
                Phone = p.Phone,
                Notes = p.Notes,
                IsActive = true
            });
        }

        // 7. Add Risk Objects
        foreach (var r in request.RiskObjects)
        {
            claim.RiskObjects.Add(new ClaimRiskObject
            {
                AssetType = Enum.Parse<AssetType>(r.AssetType),
                AssetDescription = r.AssetDescription,
                DamageDescription = r.DamageDescription,
                IsPrimary = r.IsPrimary,
                AssetReference = r.AssetReference
            });
        }

        // 8. Add Initial Reserve (if provided and policy is linked)
        if (request.InitialReserve != null && request.PolicyId.HasValue)
        {
            var compType = Enum.Parse<ReserveComponentType>(request.InitialReserve.Component);
            var amount = request.InitialReserve.Amount;

            var component = new ClaimReserveComponent
            {
                ClaimId = claim.ClaimId,
                Component = compType,
                CurrentAmount = 0, // Will be updated by approved history
                Status = "Active"
            };

            // BR-R-02: Reserve threshold checks
            var isAutoApproved = amount <= 10000;
            var approvalStatus = isAutoApproved ? ApprovalStatus.AutoApproved : ApprovalStatus.PendingApproval;

            var history = new ReserveHistory
            {
                ClaimId = claim.ClaimId,
                TransactionType = TransactionType.Add,
                Amount = amount,
                PreviousBalance = 0,
                NewBalance = isAutoApproved ? amount : 0, // Balance changes only if approved
                ApprovalStatus = approvalStatus,
                ChangeReason = request.InitialReserve.ChangeReason,
                PostingStatus = isAutoApproved ? PostingStatus.Pending : PostingStatus.Cancelled, // Pending for GL Posting job if approved
                IdempotencyKey = $"Reserve:Initial:{Guid.NewGuid()}:Change:1",
                ChangeSequence = 1,
                SubmittedByUserId = request.CurrentUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (isAutoApproved)
            {
                component.CurrentAmount = amount;
            }

            component.History.Add(history);
            claim.ReserveComponents.Add(component);
        }

        // 9. Add Domain Event
        claim.AddDomainEvent(new ClaimCreatedEvent(claim.ClaimId, claim.ClaimNumber));

        // 10. Save to DB
        await _unitOfWork.Claims.AddAsync(claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 11. Write Audit logs
        await _auditLogService.LogAsync(
            claim.ClaimId,
            "CLAIM_CREATED",
            $"Claim {claim.ClaimNumber} was created successfully.",
            null,
            System.Text.Json.JsonSerializer.Serialize(_mapper.Map<ClaimDetailDto>(claim)),
            claim.ClaimId,
            "Claim",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        // Log warnings as audit issues
        foreach (var warning in warnings)
        {
            await _auditLogService.LogAsync(
                claim.ClaimId,
                "VALIDATION_ISSUE_ADDED",
                $"Warning during intake: {warning}",
                null,
                null,
                claim.ClaimId,
                "Claim",
                request.CurrentUserId,
                request.CorrelationId,
                cancellationToken);
        }

        var detailDto = _mapper.Map<ClaimDetailDto>(claim);

        return warnings.Count > 0 
            ? Result<ClaimDetailDto>.WithWarnings(detailDto, warnings)
            : Result<ClaimDetailDto>.Success(detailDto);
    }
}
// Stub events class trigger if needed or handled directly
