using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class CauseOfLossCodeConfiguration : IEntityTypeConfiguration<CauseOfLossCode>
{
    public void Configure(EntityTypeBuilder<CauseOfLossCode> builder)
    {
        builder.ToTable("CauseOfLossCodes");

        builder.HasKey(e => e.CauseOfLossCodeId);
        builder.Property(e => e.CauseOfLossCodeId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.PerilCategory)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.SortOrder)
            .HasDefaultValue(0);
    }
}
