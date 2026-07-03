using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Commands.TransitionClaimStatus;

public record TransitionClaimStatusCommand : IRequest<Result<ClaimDetailDto>>
{
    public Guid ClaimId { get; init; }
    public string TargetStatus { get; init; } = null!;
    public string? Reason { get; init; }

    // User context properties
    public Guid CurrentUserId { get; init; }
    public string UserRole { get; init; } = "handler"; // e.g. handler, supervisor, manager
    public Guid? CorrelationId { get; init; }
}
