using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ClaimsModule.Domain.Entities;

namespace ClaimsModule.Persistence.Configurations;

public class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
{
    public void Configure(EntityTypeBuilder<ClaimDocument> builder)
    {
        builder.ToTable("ClaimDocuments");

        builder.HasKey(e => e.ClaimDocumentId);
        builder.Property(e => e.ClaimDocumentId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DocumentName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.BlobPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FileSizeBytes)
            .IsRequired();

        builder.Property(e => e.UploadedAt)
            .IsRequired();

        builder.Property(e => e.UploadedByUserId);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);
    }
}
