using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ClaimsModule.Application.DTOs.Policies;
using ClaimsModule.Application.Features.Policies.Queries.SearchPolicies;
using ClaimsModule.Application.Features.Policies.Queries.GetPolicyCoverage;

namespace ClaimsModule.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PoliciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<PolicySearchResultDto>>> Search([FromQuery] string? query)
    {
        var result = await _mediator.Send(new SearchPoliciesQuery(query));
        return Ok(result);
    }

    [HttpGet("{id}/coverage")]
    public async Task<ActionResult<IEnumerable<PolicyCoverageDto>>> GetCoverage(Guid id)
    {
        var result = await _mediator.Send(new GetPolicyCoverageQuery(id));
        return Ok(result);
    }
}
