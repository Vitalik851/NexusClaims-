using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.CreateClaim;

public record CreateClaimCommand : IRequest<Result<ClaimDetailDto>>
{
    public Guid OrganizationEntityId { get; init; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public Guid? PolicyId { get; init; }
    public DateTimeOffset LossDate { get; init; }
    public string LossDescription { get; init; } = null!;
    public string? LossLocation { get; init; }
    public string CauseOfLossCode { get; init; } = null!;
    public decimal? EstimatedLossAmount { get; init; }
    public string? Severity { get; init; }
    public string? Notes { get; init; }

    public List<CreatePartyRequest> Parties { get; init; } = [];
    public List<CreateRiskObjectRequest> RiskObjects { get; init; } = [];
    public CreateInitialReserveRequest? InitialReserve { get; init; }

    // Audit fields passed from HTTP context
    public Guid? CurrentUserId { get; init; }
    public Guid? CorrelationId { get; init; }
}

public record CreatePartyRequest
{
    public string PartyRole { get; init; } = null!;
    public string PartyType { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? CompanyName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Notes { get; init; }
}

public record CreateRiskObjectRequest
{
    public string AssetType { get; init; } = null!;
    public string AssetDescription { get; init; } = null!;
    public string? DamageDescription { get; init; }
    public bool IsPrimary { get; init; }
    public string? AssetReference { get; init; }
}

public record CreateInitialReserveRequest
{
    public string Component { get; init; } = null!;
    public decimal Amount { get; init; }
    public string ChangeReason { get; init; } = "Initial reserve set during intake";
}
