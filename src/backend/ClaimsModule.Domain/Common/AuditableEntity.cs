namespace ClaimsModule.Domain.Common;

/// <summary>
/// Extends <see cref="BaseEntity"/> with soft-delete support.
/// Business entities that should never be physically removed inherit from this class.
/// </summary>
public abstract class AuditableEntity : BaseEntity, ISoftDeletable
{
    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public void SoftDelete(DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedAt = deletedAt;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
