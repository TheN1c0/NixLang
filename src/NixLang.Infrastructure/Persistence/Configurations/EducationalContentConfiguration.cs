using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class EducationalContentConfiguration : IEntityTypeConfiguration<EducationalContent>
{
    public void Configure(EntityTypeBuilder<EducationalContent> builder)
    {
        builder.ToTable("educational_contents");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.Summary)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.Body)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.ReferenceLevel)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);
    }
}
