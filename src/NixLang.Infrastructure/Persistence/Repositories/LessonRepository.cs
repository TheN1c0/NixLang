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

    public async Task<IReadOnlyList<Lesson>> GetPublishedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons
            .AsNoTracking()
            .Where(l => l.Status == PublicationStatus.Published)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons
            .CountAsync(l => l.Status == PublicationStatus.Published, cancellationToken);
    }

    public async Task<Lesson?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Lessons
            .Include(l => l.Exercises)
            .FirstOrDefaultAsync(l => l.Id == id && l.Status == PublicationStatus.Published, cancellationToken);
    }
}
