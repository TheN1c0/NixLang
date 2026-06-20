using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("exercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.LessonId)
            .IsRequired();

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Statement)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.CorrectAnswer)
            .HasMaxLength(1000);

        builder.Property(e => e.DisplayOrder)
            .IsRequired();

        builder.Property(e => e.AudioResourceUrl)
            .HasMaxLength(500);

        builder.HasMany<ExerciseOption>()
            .WithOne()
            .HasForeignKey(o => o.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
