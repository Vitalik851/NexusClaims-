using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimAuditLogConfiguration : IEntityTypeConfiguration<ClaimAuditLog>
{
    public void Configure(EntityTypeBuilder<ClaimAuditLog> builder)
    {
        builder.ToTable("ClaimAuditLogs");

        builder.HasKey(e => e.AuditLogId);
        builder.Property(e => e.AuditLogId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.OldValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.NewValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.RelatedEntityId);

        builder.Property(e => e.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(e => e.CorrelationId);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.CreatedByUserId);
    }
}
