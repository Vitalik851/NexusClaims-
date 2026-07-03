using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Commands.RejectReserve;

public record RejectReserveCommand : IRequest<Result<ReserveHistoryDto>>
{
    public Guid ReserveHistoryId { get; init; }
    public string RejectionReason { get; init; } = null!;

    // User context
    public Guid CurrentUserId { get; init; }
    public string UserRole { get; init; } = "handler";
    public Guid? CorrelationId { get; init; }
}
