namespace ClaimsModule.Domain.Common;

/// <summary>
/// Marker interface for entities that support soft deletion.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }

    void SoftDelete(DateTimeOffset deletedAt);
    void Restore();
}
