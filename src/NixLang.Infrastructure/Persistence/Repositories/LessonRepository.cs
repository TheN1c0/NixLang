using Microsoft.EntityFrameworkCore;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Infrastructure.Persistence.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly NixLangDbContext _dbContext;

    public LessonRepository(NixLangDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Lesson>> GetPublishedAsync(
        int page, 
        int pageSize, 
        string? search = null, 
        string? level = null, 
        IEnumerable<Guid>? categoryIds = null, 
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Lessons.AsNoTracking(), search, level, categoryIds);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPublishedAsync(
        string? search = null, 
        string? level = null, 
        IEnumerable<Guid>? categoryIds = null, 
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Lessons, search, level, categoryIds);
        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<Lesson> ApplyFilters(
        IQueryable<Lesson> query, 
        string? search, 
        string? level, 
        IEnumerable<Guid>? categoryIds)
    {
        query = query.Where(l => l.Status == PublicationStatus.Published);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTermLower = search.Trim().ToLower();
            query = query.Where(l => l.Title.ToLower().Contains(searchTermLower) || 
                                     l.Description.ToLower().Contains(searchTermLower));
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            if (Enum.TryParse<ReferenceLevel>(level, true, out var refLevel))
            {
                query = query.Where(l => l.ReferenceLevel == refLevel);
            }
        }

        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(l => l.Categories.Any(c => categoryIds.Contains(c.Id)));
        }

        return query;
    }

    public async Task<Lesson?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons
            .Include(l => l.LessonBlocks)
                .ThenInclude(b => b.Exercise)
                    .ThenInclude(e => e!.Options)
            .Include(l => l.Categories)
            .FirstOrDefaultAsync(l => l.Id == id && l.Status == PublicationStatus.Published, cancellationToken);
    }
}
