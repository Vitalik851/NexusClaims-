namespace ClaimsModule.Application.DTOs.Reserves;

public class ReserveComponentSummaryDto
{
    public Guid ReserveComponentId { get; set; }
    public string Component { get; set; } = null!;
    public decimal CurrentAmount { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
}
