using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class ReserveHistoryConfiguration : IEntityTypeConfiguration<ReserveHistory>
{
    public void Configure(EntityTypeBuilder<ReserveHistory> builder)
    {
        builder.ToTable("ReserveHistories");

        builder.HasKey(e => e.ReserveHistoryId);
        builder.Property(e => e.ReserveHistoryId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Amount)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(e => e.PreviousBalance)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(e => e.NewBalance)
            .IsRequired()
            .HasPrecision(19, 4);

        builder.Property(e => e.ApprovalStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ApprovedByUserId);
        builder.Property(e => e.ApprovedAt);

        builder.Property(e => e.RejectedByUserId);
        builder.Property(e => e.RejectedAt);
        builder.Property(e => e.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(e => e.ChangeReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.PostingStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(e => e.PostingJobId)
            .HasMaxLength(100);

        builder.Property(e => e.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.IdempotencyKey)
            .IsUnique();

        builder.Property(e => e.ChangeSequence)
            .IsRequired();

        builder.Property(e => e.SubmittedByUserId);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // FK to Claim (denormalized)
        builder.HasOne<Claim>()
            .WithMany()
            .HasForeignKey(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
