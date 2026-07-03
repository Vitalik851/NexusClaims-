namespace ClaimsModule.Application.DTOs.Claims;

public class ClaimPartyDto
{
    public Guid ClaimPartyId { get; set; }
    public string PartyRole { get; set; } = null!;
    public string PartyType { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
