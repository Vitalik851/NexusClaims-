using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using ClaimsModule.Application.Common.Models;
using ClaimsModule.Application.DTOs.Claims;
using ClaimsModule.Application.DTOs.Documents;
using ClaimsModule.Application.DTOs.Audit;
using ClaimsModule.Application.Features.Claims.Commands.CreateClaim;
using ClaimsModule.Application.Features.Claims.Commands.TransitionClaimStatus;
using ClaimsModule.Application.Features.Claims.Commands.AddClaimParty;
using ClaimsModule.Application.Features.Claims.Commands.RemoveClaimParty;
using ClaimsModule.Application.Features.Claims.Queries.ListClaims;
using ClaimsModule.Application.Features.Claims.Queries.GetClaimDetail;
using ClaimsModule.Application.Features.Claims.Queries.GetClaimAuditLog;
using ClaimsModule.Application.Features.Documents.Commands.UploadDocument;

namespace ClaimsModule.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "00000000-0000-0000-0000-000000000001");
    private string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "handler";
    private Guid TenantId => Guid.Parse(User.FindFirstValue("OrganizationId") ?? "11111111-1111-1111-1111-111111111111");

    [HttpPost]
    public async Task<ActionResult<Result<ClaimDetailDto>>> Create([FromBody] CreateClaimCommand command)
    {
        // Enrich command with authentication context
        var enrichedCommand = command with 
        { 
            OrganizationEntityId = TenantId,
            CurrentUserId = CurrentUserId, 
            CorrelationId = Guid.NewGuid() 
        };

        var result = await _mediator.Send(enrichedCommand);
        
        if (!result.IsSuccess)
        {
            return UnprocessableEntity(result); // 422
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.ClaimId }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ClaimSummaryDto>>> GetAll([FromQuery] ListClaimsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimDetailDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetClaimDetailQuery(id));
        return Ok(result);
    }

    [HttpPost("{id}/status")]
    public async Task<ActionResult<Result<ClaimDetailDto>>> TransitionStatus(Guid id, [FromBody] StatusTransitionRequestDto request)
    {
        var command = new TransitionClaimStatusCommand
        {
            ClaimId = id,
            TargetStatus = request.TargetStatus,
            Reason = request.Reason,
            CurrentUserId = CurrentUserId,
            UserRole = UserRole,
            CorrelationId = Guid.NewGuid()
        };

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/parties")]
    public async Task<ActionResult<Result<ClaimDetailDto>>> AddParty(Guid id, [FromBody] AddPartyRequestDto request)
    {
        var command = new AddClaimPartyCommand
        {
            ClaimId = id,
            PartyRole = request.PartyRole,
            PartyType = request.PartyType,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone,
            Notes = request.Notes,
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

    [HttpDelete("{id}/parties/{partyId}")]
    public async Task<ActionResult<Result<ClaimDetailDto>>> RemoveParty(Guid id, Guid partyId)
    {
        var command = new RemoveClaimPartyCommand
        {
            ClaimId = id,
            ClaimPartyId = partyId,
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

    [HttpGet("{id}/audit")]
    public async Task<ActionResult<PaginatedList<AuditLogEntryDto>>> GetAuditLog(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetClaimAuditLogQuery(id, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpPost("{id}/documents")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<ClaimDocumentDto>>> UploadDocument(Guid id, [FromForm] UploadDocumentRequestDto request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        using var stream = request.File.OpenReadStream();
        var command = new UploadDocumentCommand
        {
            ClaimId = id,
            DocumentType = request.DocumentType,
            DocumentName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSizeBytes = request.File.Length,
            FileStream = stream,
            Notes = request.Notes,
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
}

public record StatusTransitionRequestDto(string TargetStatus, string? Reason);
public record AddPartyRequestDto(string PartyRole, string PartyType, string? FirstName, string? LastName, string? CompanyName, string? Email, string? Phone, string? Notes);
public class UploadDocumentRequestDto
{
    public string DocumentType { get; set; } = null!;
    public IFormFile File { get; set; } = null!;
    public string? Notes { get; set; }
}
