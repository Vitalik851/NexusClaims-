using MediatR;
using AutoMapper;
using ClaimsModule.Domain.Entities;
using ClaimsModule.Domain.Interfaces;
using ClaimsModule.Domain.Enumerations;
using ClaimsModule.Domain.Events;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.Common.Exceptions;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.AddClaimParty;

public class AddClaimPartyCommandHandler : IRequestHandler<AddClaimPartyCommand, Result<ClaimDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;

    public AddClaimPartyCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ClaimDetailDto>> Handle(AddClaimPartyCommand request, CancellationToken cancellationToken)
    {
        var claim = await _unitOfWork.Claims.GetByIdWithDetailsAsync(request.ClaimId, cancellationToken);
        if (claim == null)
        {
            throw new NotFoundException(nameof(Claim), request.ClaimId);
        }

        var party = claim.AddParty(
            Enum.Parse<PartyRole>(request.PartyRole),
            Enum.Parse<PartyType>(request.PartyType),
            request.FirstName,
            request.LastName,
            request.CompanyName,
            request.Email,
            request.Phone,
            request.Notes
        );

        _unitOfWork.Claims.Update(claim);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var partyName = party.PartyType == PartyType.Person 
            ? $"{party.FirstName} {party.LastName}" 
            : party.CompanyName;

        await _auditLogService.LogAsync(
            claim.ClaimId,
            "PARTY_ADDED",
            $"Added new party {partyName} ({party.PartyRole}) to the claim.",
            null,
            System.Text.Json.JsonSerializer.Serialize(_mapper.Map<ClaimPartyDto>(party)),
            party.Id,
            "ClaimParty",
            request.CurrentUserId,
            request.CorrelationId,
            cancellationToken);

        var detailDto = _mapper.Map<ClaimDetailDto>(claim);
        return Result<ClaimDetailDto>.Success(detailDto);
    }
}
