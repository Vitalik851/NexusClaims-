using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.AddClaimParty;

public record AddClaimPartyCommand : IRequest<Result<ClaimDetailDto>>
{
    public Guid ClaimId { get; init; }
    public string PartyRole { get; init; } = null!;
    public string PartyType { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }

    // User context
    public Guid CurrentUserId { get; init; }
    public Guid? CorrelationId { get; init; }
}
