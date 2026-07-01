using Microsoft.EntityFrameworkCore;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;

namespace NixLang.Infrastructure.Persistence;

public class NixLangDbContext : DbContext, IUnitOfWork
{
    public NixLangDbContext(DbContextOptions<NixLangDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonBlock> LessonBlocks => Set<LessonBlock>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseOption> ExerciseOptions => Set<ExerciseOption>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
    public DbSet<ExerciseResult> ExerciseResults => Set<ExerciseResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NixLangDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
