using Microsoft.EntityFrameworkCore;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;

namespace NixLang.Infrastructure.Persistence.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly NixLangDbContext _dbContext;

    public ExerciseRepository(NixLangDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Exercise>> GetAllAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Exercises.Include(e => e.Options).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(e => e.Statement.ToLower().Contains(searchLower));
        }

        return await query
            .OrderBy(e => e.Statement)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Exercises.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(e => e.Statement.ToLower().Contains(searchLower));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Exercises
            .Include(e => e.Options)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> IsExerciseUsedInLessonAsync(Guid exerciseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LessonBlocks
            .AnyAsync(b => b.ReferencedExerciseId == exerciseId, cancellationToken);
    }

    public async Task AddAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        await _dbContext.Exercises.AddAsync(exercise, cancellationToken);
    }

    public async Task DeleteAsync(Exercise exercise, CancellationToken cancellationToken = default)
    {
        _dbContext.Exercises.Remove(exercise);
        await Task.CompletedTask;
    }

    public async Task ClearOptionsAsync(Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var options = await _dbContext.ExerciseOptions
            .Where(o => o.ExerciseId == exerciseId)
            .ToListAsync(cancellationToken);
        _dbContext.ExerciseOptions.RemoveRange(options);
    }

    public async Task AddOptionAsync(ExerciseOption option, CancellationToken cancellationToken = default)
    {
        await _dbContext.ExerciseOptions.AddAsync(option, cancellationToken);
    }
}
