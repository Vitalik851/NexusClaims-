using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.RemoveClaimParty;

public record RemoveClaimPartyCommand : IRequest<Result<ClaimDetailDto>>
{
    public Guid ClaimId { get; init; }
    public Guid ClaimPartyId { get; init; }

    // User context
    public Guid CurrentUserId { get; init; }
    public Guid? CorrelationId { get; init; }
}
