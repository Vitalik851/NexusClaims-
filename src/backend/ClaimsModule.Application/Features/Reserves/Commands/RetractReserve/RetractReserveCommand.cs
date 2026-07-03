using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Commands.RetractReserve;

public record RetractReserveCommand : IRequest<Result<ReserveHistoryDto>>
{
    public Guid ReserveHistoryId { get; init; }

    // User context
    public Guid CurrentUserId { get; init; }
    public Guid? CorrelationId { get; init; }
}
