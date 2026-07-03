namespace ClaimsModule.Application.DTOs.Claims;

public class LossEventDto
{
    public DateTimeOffset LossDate { get; set; }
    public string LossDescription { get; set; } = null!;
    public string? LossLocation { get; set; }
    public string CauseOfLossCode { get; set; } = null!;
    public string CauseOfLossName { get; set; } = null!;
    public decimal? EstimatedLossAmount { get; set; }
    public DateTimeOffset ReportDate { get; set; }
    public string? PoliceReportNumber { get; set; }
}
