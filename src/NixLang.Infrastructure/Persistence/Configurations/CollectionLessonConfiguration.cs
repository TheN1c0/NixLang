using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class CollectionLessonConfiguration : IEntityTypeConfiguration<CollectionLesson>
{
    public void Configure(EntityTypeBuilder<CollectionLesson> builder)
    {
        builder.ToTable("collection_lessons");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.CollectionId)
            .IsRequired();

        builder.Property(cl => cl.LessonId)
            .IsRequired();

        builder.Property(cl => cl.Order)
            .IsRequired();

        builder.HasOne(cl => cl.Lesson)
            .WithMany()
            .HasForeignKey(cl => cl.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cl => new { cl.CollectionId, cl.LessonId })
            .IsUnique();

        builder.HasIndex(cl => new { cl.CollectionId, cl.Order });
    }
}
