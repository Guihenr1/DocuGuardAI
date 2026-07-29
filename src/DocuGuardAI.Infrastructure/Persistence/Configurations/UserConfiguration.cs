using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocuGuardAI.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique(); 

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);
        
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);
        
        builder.Property(u => u.Role)
            .HasConversion<int>()
            .HasDefaultValue(UserRole.Viewer);

        builder.ToTable("Users");
    }
}