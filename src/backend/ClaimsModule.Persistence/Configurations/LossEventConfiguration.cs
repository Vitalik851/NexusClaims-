using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class LossEventConfiguration : IEntityTypeConfiguration<LossEvent>
{
    public void Configure(EntityTypeBuilder<LossEvent> builder)
    {
        builder.ToTable("LossEvents");

        builder.HasKey(e => e.LossEventId);
        builder.Property(e => e.LossEventId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.LossDescription)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.LossLocation)
            .HasMaxLength(500);

        builder.Property(e => e.CauseOfLossCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.EstimatedLossAmount)
            .HasPrecision(19, 4);

        builder.Property(e => e.PoliceReportNumber)
            .HasMaxLength(100);

        // Map foreign key relationship with CauseOfLossCode if required
        builder.HasOne(e => e.CauseOfLossCodeNavigation)
            .WithMany()
            .HasPrincipalKey(c => c.Code)
            .HasForeignKey(e => e.CauseOfLossCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
