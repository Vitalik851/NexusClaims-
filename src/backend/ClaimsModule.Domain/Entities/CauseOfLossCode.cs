using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Entities;

/// <summary>
/// Lookup entity for standardised cause-of-loss codes used in loss events.
/// </summary>
public class CauseOfLossCode : AuditableEntity
{
    public Guid CauseOfLossCodeId { get => Id; set => Id = value; }

    /// <summary>
    /// Short unique code (e.g., "FIRE", "THEFT", "FLOOD").
    /// </summary>
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? PerilCategory { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }

    // ──── Navigation Properties ────

    public List<LossEvent> LossEvents { get; set; } = [];
}
