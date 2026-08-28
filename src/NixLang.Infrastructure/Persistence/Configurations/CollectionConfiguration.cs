using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("collections");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(500);

        builder.Property(c => c.SuggestedLevel)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasMany(c => c.CollectionLessons)
            .WithOne()
            .HasForeignKey(cl => cl.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.CollectionLessons)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
