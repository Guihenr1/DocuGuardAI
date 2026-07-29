using DocuGuardAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocuGuardAI.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.FilePath)
            .IsRequired();

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Documents)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.UploadedAt);
    }
}