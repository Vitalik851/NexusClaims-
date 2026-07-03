namespace ClaimsModule.Application.DTOs.Audit;

public class AuditLogEntryDto
{
    public Guid AuditLogId { get; set; }
    public Guid ClaimId { get; set; }
    public string EventType { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? CorrelationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
