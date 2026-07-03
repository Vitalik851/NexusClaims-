using ClaimsModule.Application.DTOs.Documents;
using ClaimsModule.Application.DTOs.Reserves;
using ClaimsModule.Application.DTOs.Audit;

namespace ClaimsModule.Application.DTOs.Claims;

public class ClaimDetailDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = null!;
    public Guid OrganizationEntityId { get; set; }
    public Guid? PolicyId { get; set; }
    public string? PolicyNumber { get; set; }
    public string? ClientName { get; set; }
    public string Status { get; set; } = null!;
    public string? Severity { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
    public Guid? AssignedHandlerId { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public string? ClosureReason { get; set; }
    public string? Notes { get; set; }
    public bool ManagerOverrideFlag { get; set; }

    public LossEventDto LossEvent { get; set; } = null!;
    public IEnumerable<ClaimPartyDto> Parties { get; set; } = Array.Empty<ClaimPartyDto>();
    public IEnumerable<ClaimRiskObjectDto> RiskObjects { get; set; } = Array.Empty<ClaimRiskObjectDto>();
    public IEnumerable<ReserveComponentSummaryDto> ReserveComponents { get; set; } = Array.Empty<ReserveComponentSummaryDto>();
    public IEnumerable<ClaimDocumentDto> Documents { get; set; } = Array.Empty<ClaimDocumentDto>();
    public IEnumerable<AuditLogEntryDto> RecentAuditEntries { get; set; } = Array.Empty<AuditLogEntryDto>();
}
