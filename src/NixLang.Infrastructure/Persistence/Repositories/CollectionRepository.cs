using Microsoft.EntityFrameworkCore;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Infrastructure.Persistence.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly NixLangDbContext _dbContext;

    public CollectionRepository(NixLangDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Collection>> GetPublishedAsync(
        int page, 
        int pageSize, 
        string? search = null, 
        string? level = null, 
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Collections.AsNoTracking(), search, level);

        return await query
            .Include(c => c.CollectionLessons)
                .ThenInclude(cl => cl.Lesson)
            .OrderBy(c => c.DisplayOrder)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPublishedAsync(
        string? search = null, 
        string? level = null, 
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Collections, search, level);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<Collection?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Collections
            .Include(c => c.CollectionLessons)
                .ThenInclude(cl => cl.Lesson)
            .FirstOrDefaultAsync(c => c.Id == id && c.Status == PublicationStatus.Published, cancellationToken);
    }

    public async Task<IReadOnlyList<Collection>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Collections
            .AsNoTracking()
            .Include(c => c.CollectionLessons)
                .ThenInclude(cl => cl.Lesson)
            .OrderBy(c => c.DisplayOrder)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Collections.CountAsync(cancellationToken);
    }

    public async Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Collections
            .Include(c => c.CollectionLessons)
                .ThenInclude(cl => cl.Lesson)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        await _dbContext.Collections.AddAsync(collection, cancellationToken);
    }

    public async Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default)
    {
        _dbContext.Collections.Remove(collection);
        await Task.CompletedTask;
    }

    private IQueryable<Collection> ApplyFilters(
        IQueryable<Collection> query, 
        string? search, 
        string? level)
    {
        query = query.Where(c => c.Status == PublicationStatus.Published);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(term) || 
                                     c.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            if (Enum.TryParse<ReferenceLevel>(level, true, out var refLevel))
            {
                query = query.Where(c => c.SuggestedLevel == refLevel);
            }
        }

        return query;
    }
}
