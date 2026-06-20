using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class ExerciseResultConfiguration : IEntityTypeConfiguration<ExerciseResult>
{
    public void Configure(EntityTypeBuilder<ExerciseResult> builder)
    {
        builder.ToTable("exercise_results");

        builder.HasKey(er => er.Id);

        builder.Property(er => er.LessonProgressId)
            .IsRequired();

        builder.Property(er => er.ExerciseId)
            .IsRequired();

        builder.Property(er => er.GivenAnswer)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(er => er.IsCorrect)
            .IsRequired();

        builder.Property(er => er.AnsweredAt)
            .IsRequired();

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(er => er.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
