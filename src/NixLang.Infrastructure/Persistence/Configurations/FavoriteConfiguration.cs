using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorites");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.UserId)
            .IsRequired();

        builder.Property(f => f.LessonId)
            .IsRequired();

        builder.Property(f => f.MarkedAt)
            .IsRequired();

        builder.HasIndex(f => new { f.UserId, f.LessonId })
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(f => f.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
