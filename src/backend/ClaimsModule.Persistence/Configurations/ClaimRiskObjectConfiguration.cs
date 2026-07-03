using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimRiskObjectConfiguration : IEntityTypeConfiguration<ClaimRiskObject>
{
    public void Configure(EntityTypeBuilder<ClaimRiskObject> builder)
    {
        builder.ToTable("ClaimRiskObjects");

        builder.HasKey(e => e.ClaimRiskObjectId);
        builder.Property(e => e.ClaimRiskObjectId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.AssetType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.AssetDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.DamageDescription)
            .HasMaxLength(2000);

        builder.Property(e => e.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(e => e.AssetReference)
            .HasMaxLength(255);
    }
}
