using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.RemoveClaimParty;

public class RemoveClaimPartyCommandHandler : IRequestHandler<RemoveClaimPartyCommand, Result<ClaimDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public RemoveClaimPartyCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ClaimDetailDto>> Handle(RemoveClaimPartyCommand request, CancellationToken cancellationToken)
    {
        var claim = await _unitOfWork.Claims.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(Claim), request.ClaimId);
        }

        var party = claim.Parties.FirstOrDefault(p => p.ClaimPartyId == request.ClaimPartyId && p.IsActive);
        if (party == null)
        {
            throw new NotFoundException(nameof(ClaimParty), request.ClaimPartyId);
        }

        // BR-P-01 / BR-C-03: Cannot remove the last Claimant party from the claim
        if (party.PartyRole == PartyRole.Claimant)
        {
            var activeClaimantsCount = claim.Parties.Count(p => p.PartyRole == PartyRole.Claimant && p.IsActive);
            if (activeClaimantsCount <= 1)
            {
                return Result<ClaimDetailDto>.Failure("Cannot remove the party: A claim must have at least one active Claimant (BR-P-01).");
            }
        }

        claim.RemoveParty(party.ClaimPartyId);

        _unitOfWork.Claims.Update(claim);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var partyName = party.PartyType == PartyType.Person 
            ? $"{party.FirstName} {party.LastName}" 
            : party.CompanyName;

        await _auditLogService.LogAsync(
            claim.ClaimId,
            "PARTY_REMOVED",
            $"Soft-removed party {partyName} ({party.PartyRole}) from the claim.",
            System.Text.Json.JsonSerializer.Serialize(_mapper.Map<ClaimPartyDto>(party)),
            null,
            party.ClaimPartyId,
            "ClaimParty",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        var detailDto = _mapper.Map<ClaimDetailDto>(claim);
        return Result<ClaimDetailDto>.Success(detailDto);
    }
}
