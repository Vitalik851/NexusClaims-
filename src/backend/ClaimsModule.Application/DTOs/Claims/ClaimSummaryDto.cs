namespace ClaimsModule.Application.DTOs.Claims;

public class ClaimSummaryDto
{
    public Guid ClaimId { get; set; }
    public string ClaimNumber { get; set; } = null!;
    public string? PolicyNumber { get; set; }
    public string? ClientName { get; set; }
    public DateTimeOffset LossDate { get; set; }
    public string CauseOfLossCode { get; set; } = null!;
    public string CauseOfLossName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal TotalReserves { get; set; }
    public Guid? AssignedHandlerId { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
}
