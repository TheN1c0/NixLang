using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(l => l.ReferenceLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.HasMany(l => l.LessonBlocks)
            .WithOne()
            .HasForeignKey(b => b.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.LessonBlocks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(l => l.Categories)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "LessonCategory",
                j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId").HasConstraintName("fk_lesson_categories_categories_category_id"),
                j => j.HasOne<Lesson>().WithMany().HasForeignKey("LessonId").HasConstraintName("fk_lesson_categories_lessons_lesson_id"),
                j =>
                {
                    j.ToTable("lesson_categories");
                    j.HasKey("LessonId", "CategoryId");
                });

        builder.Navigation(l => l.Categories)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
