using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Reserves;
using ClaimsModule.Application.Features.Reserves.Commands.CreateReserve;
using ClaimsModule.Application.Features.Reserves.Commands.ApproveReserve;
using ClaimsModule.Application.Features.Reserves.Commands.RejectReserve;
using ClaimsModule.Application.Features.Reserves.Commands.RetractReserve;
using ClaimsModule.Application.Features.Reserves.Queries.ListReserves;

namespace ClaimsModule.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReservesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "00000000-0000-0000-0000-000000000001");
    private string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "handler";

    [HttpPost]
    public async Task<ActionResult<Result<ReserveHistoryDto>>> Create([FromBody] CreateReserveRequestDto request)
    {
        var command = new CreateReserveCommand
        {
            ClaimId = request.ClaimId,
            Component = request.Component,
            Amount = request.Amount,
            TransactionType = request.TransactionType,
            ChangeReason = request.ChangeReason,
            CurrentUserId = CurrentUserId,
            UserRole = UserRole,
            CorrelationId = Guid.NewGuid()
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return UnprocessableEntity(result); // 422
        }

        return Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<Result<ReserveHistoryDto>>> Approve(Guid id)
    {
        var command = new ApproveReserveCommand
        {
            ReserveHistoryId = id,
            CurrentUserId = CurrentUserId,
            UserRole = UserRole,
            CorrelationId = Guid.NewGuid()
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return UnprocessableEntity(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<Result<ReserveHistoryDto>>> Reject(Guid id, [FromBody] RejectReserveRequestDto request)
    {
        var command = new RejectReserveCommand
        {
            ReserveHistoryId = id,
            RejectionReason = request.RejectionReason,
            CurrentUserId = CurrentUserId,
            UserRole = UserRole,
            CorrelationId = Guid.NewGuid()
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return UnprocessableEntity(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/retract")]
    public async Task<ActionResult<Result<ReserveHistoryDto>>> Retract(Guid id)
    {
        var command = new RetractReserveCommand
        {
            ReserveHistoryId = id,
            CurrentUserId = CurrentUserId,
            CorrelationId = Guid.NewGuid()
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return UnprocessableEntity(result);
        }

        return Ok(result);
    }

    [HttpGet("claim/{claimId}")]
    public async Task<ActionResult<ReserveSummaryDto>> GetByClaimId(Guid claimId)
    {
        var result = await _mediator.Send(new ListReservesQuery(claimId));
        return Ok(result);
    }
}

public record CreateReserveRequestDto(Guid ClaimId, string Component, decimal Amount, string TransactionType, string ChangeReason);
public record RejectReserveRequestDto(string RejectionReason);
