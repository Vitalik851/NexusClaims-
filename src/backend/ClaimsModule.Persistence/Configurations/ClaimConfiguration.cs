using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        builder.HasKey(e => e.ClaimId);
        builder.Property(e => e.ClaimId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.ClaimNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.ClaimNumber)
            .IsUnique();

        builder.Property(e => e.PolicyNumber)
            .HasMaxLength(50);

        builder.Property(e => e.ClientName)
            .HasMaxLength(255);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Severity)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ReportedDate)
            .IsRequired();

        builder.Property(e => e.ClosedAt);
        builder.Property(e => e.ClosureReason)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.ManagerOverrideFlag)
            .HasDefaultValue(false);

        builder.Property(e => e.OrganizationEntityId)
            .IsRequired();

        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Relationships
        builder.HasOne(e => e.LossEvent)
            .WithOne(e => e.Claim)
            .HasForeignKey<LossEvent>(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Parties)
            .WithOne(e => e.Claim)
            .HasForeignKey(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RiskObjects)
            .WithOne(e => e.Claim)
            .HasForeignKey(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ReserveComponents)
            .WithOne(e => e.Claim)
            .HasForeignKey(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Documents)
            .WithOne(e => e.Claim)
            .HasForeignKey(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.AuditLogs)
            .WithOne(e => e.Claim)
            .HasForeignKey(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
