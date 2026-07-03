using MediatR;
using ClaimsModule.Application.DTOs.Reference;

namespace ClaimsModule.Application.Features.Reference.Queries.ListCauseOfLossCodes;

public record ListCauseOfLossCodesQuery(string? PerilCategory = null) : IRequest<IEnumerable<CauseOfLossCodeDto>>;
