namespace ClaimsModule.Application.DTOs.Reference;

public class ClaimStatusTransitionDto
{
    public string FromStatus { get; set; } = null!;
    public string ToStatus { get; set; } = null!;
    public string? RequiredPermission { get; set; }
}
