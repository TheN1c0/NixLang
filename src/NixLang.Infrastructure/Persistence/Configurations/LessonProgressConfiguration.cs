using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
{
    public void Configure(EntityTypeBuilder<LessonProgress> builder)
    {
        builder.ToTable("lesson_progress");

        builder.HasKey(lp => lp.Id);

        builder.Property(lp => lp.UserId)
            .IsRequired();

        builder.Property(lp => lp.LessonId)
            .IsRequired();

        builder.Property(lp => lp.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(lp => lp.ProgressPercentage)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(lp => lp.StartedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(lp => lp.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(lp => lp.ExerciseResults)
            .WithOne()
            .HasForeignKey(er => er.LessonProgressId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(lp => lp.ExerciseResults)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
