using MediatR;
using ClaimsModule.Application.DTOs.Reserves;

namespace ClaimsModule.Application.Features.Reserves.Queries.ListReserves;

public record ListReservesQuery(Guid ClaimId) : IRequest<ReserveSummaryDto>;
