namespace ClaimsModule.Application.DTOs.Reserves;

public class ReserveSummaryDto
{
    public IEnumerable<ReserveComponentSummaryDto> Components { get; set; } = Array.Empty<ReserveComponentSummaryDto>();
    public IEnumerable<ReserveHistoryDto> History { get; set; } = Array.Empty<ReserveHistoryDto>();
}
