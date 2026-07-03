using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Commands.CreateReserve;

public record CreateReserveCommand : IRequest<Result<ReserveHistoryDto>>
{
    public Guid ClaimId { get; init; }
    public string Component { get; init; } = null!;
    public decimal Amount { get; init; }
    public string TransactionType { get; init; } = null!; // Add, Adjust, Reverse
    public string ChangeReason { get; init; } = null!;

    // User context
    public Guid CurrentUserId { get; init; }
    public string UserRole { get; init; } = "handler";
    public Guid? CorrelationId { get; init; }
}
