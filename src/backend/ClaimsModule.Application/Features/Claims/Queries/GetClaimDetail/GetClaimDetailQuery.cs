using MediatR;
using ClaimsModule.Application.DTOs.Claims;

namespace ClaimsModule.Application.Features.Claims.Queries.GetClaimDetail;

public record GetClaimDetailQuery(Guid ClaimId) : IRequest<ClaimDetailDto>;
