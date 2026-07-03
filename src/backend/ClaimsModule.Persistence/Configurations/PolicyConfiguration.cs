using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(e => e.PolicyId);
        builder.Property(e => e.PolicyId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.PolicyNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.PolicyNumber)
            .IsUnique();

        builder.Property(e => e.ClientName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.EffectiveDate)
            .IsRequired();

        builder.Property(e => e.ExpirationDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.CoverageTypes)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.OrganizationEntityId)
            .IsRequired();
    }
}
