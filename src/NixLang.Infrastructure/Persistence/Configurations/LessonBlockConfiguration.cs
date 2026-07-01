using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;
using NixLang.Domain.ValueObjects;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class LessonBlockConfiguration : IEntityTypeConfiguration<LessonBlock>
{
    public void Configure(EntityTypeBuilder<LessonBlock> builder)
    {
        builder.ToTable("lesson_blocks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.LessonId)
            .IsRequired();

        builder.Property(b => b.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.Sequence)
            .IsRequired();

        builder.Property(b => b.Configuration)
            .IsRequired()
            .HasConversion(
                c => c.Value,
                v => new BlockConfiguration(v))
            .HasMaxLength(4000);

        builder.Property(b => b.ReferencedExerciseId);

        builder.HasIndex(b => new { b.LessonId, b.Sequence })
            .IsUnique();

        builder.HasOne(b => b.Exercise)
            .WithMany()
            .HasForeignKey(b => b.ReferencedExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
