using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class ExerciseOptionConfiguration : IEntityTypeConfiguration<ExerciseOption>
{
    public void Configure(EntityTypeBuilder<ExerciseOption> builder)
    {
        builder.ToTable("exercise_options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ExerciseId)
            .IsRequired();

        builder.Property(o => o.Text)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.IsCorrect)
            .IsRequired();

        builder.Property(o => o.DisplayOrder)
            .IsRequired();
    }
}
