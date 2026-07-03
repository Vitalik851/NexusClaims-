namespace ClaimsModule.Application.DTOs.Claims;

public class ClaimRiskObjectDto
{
    public Guid ClaimRiskObjectId { get; set; }
    public string AssetType { get; set; } = null!;
    public string AssetDescription { get; set; } = null!;
    public string? DamageDescription { get; set; }
    public bool IsPrimary { get; set; }
    public string? AssetReference { get; set; }
}
