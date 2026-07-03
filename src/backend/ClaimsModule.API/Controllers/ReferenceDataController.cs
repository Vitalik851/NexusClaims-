using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ClaimsModule.Application.DTOs.Reference;
using ClaimsModule.Application.Features.Reference.Queries.ListCauseOfLossCodes;
using ClaimsModule.Application.Features.Reference.Queries.ListClaimStatuses;

namespace ClaimsModule.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReferenceDataController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReferenceDataController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("cause-of-loss-codes")]
    public async Task<ActionResult<IEnumerable<CauseOfLossCodeDto>>> GetCauseOfLossCodes([FromQuery] string? perilCategory)
    {
        var result = await _mediator.Send(new ListCauseOfLossCodesQuery(perilCategory));
        return Ok(result);
    }

    [HttpGet("status-transitions")]
    public async Task<ActionResult<IEnumerable<ClaimStatusTransitionDto>>> GetStatusTransitions()
    {
        var result = await _mediator.Send(new ListClaimStatusesQuery());
        return Ok(result);
    }
}
