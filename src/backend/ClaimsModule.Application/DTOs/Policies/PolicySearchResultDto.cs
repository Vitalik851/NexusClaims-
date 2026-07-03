namespace ClaimsModule.Application.DTOs.Policies;

public class PolicySearchResultDto
{
    public Guid PolicyId { get; set; }
    public string PolicyNumber { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public DateOnly EffectiveDate { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public string Status { get; set; } = null!;
    public IEnumerable<string> CoverageTypes { get; set; } = Array.Empty<string>();
}
