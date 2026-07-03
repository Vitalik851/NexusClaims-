using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimReserveComponentConfiguration : IEntityTypeConfiguration<ClaimReserveComponent>
{
    public void Configure(EntityTypeBuilder<ClaimReserveComponent> builder)
    {
        builder.ToTable("ClaimReserveComponents");

        builder.HasKey(e => e.ReserveComponentId);
        builder.Property(e => e.ReserveComponentId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.Component)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.CurrentAmount)
            .IsRequired()
            .HasPrecision(19, 4)
            .HasDefaultValue(0.0000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Active");

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasMany(e => e.History)
            .WithOne(e => e.ReserveComponent)
            .HasForeignKey(e => e.ReserveComponentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
