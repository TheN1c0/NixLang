using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Infrastructure.Persistence.Repositories;

public class EducationalContentRepository : IEducationalContentRepository
{
    private readonly NixLangDbContext _dbContext;

    public EducationalContentRepository(NixLangDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EducationalContent>> GetPublishedAsync(
        int page,
        int pageSize,
        string? search = null,
        EducationalContentType? type = null,
        ReferenceLevel? level = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EducationalContents
            .AsNoTracking()
            .Where(c => c.Status == PublicationStatus.Published);

        query = ApplyFilters(query, search, type, level);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPublishedAsync(
        string? search = null,
        EducationalContentType? type = null,
        ReferenceLevel? level = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EducationalContents
            .Where(c => c.Status == PublicationStatus.Published);

        query = ApplyFilters(query, search, type, level);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<EducationalContent?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EducationalContents
            .FirstOrDefaultAsync(c => c.Id == id && c.Status == PublicationStatus.Published, cancellationToken);
    }

    public async Task<IReadOnlyList<EducationalContent>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        EducationalContentType? type = null,
        PublicationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EducationalContents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(searchLower) || c.Summary.ToLower().Contains(searchLower));
        }

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAllAsync(
        string? search = null,
        EducationalContentType? type = null,
        PublicationStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.EducationalContents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(searchLower) || c.Summary.ToLower().Contains(searchLower));
        }

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<EducationalContent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EducationalContents
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(EducationalContent content, CancellationToken cancellationToken = default)
    {
        await _dbContext.EducationalContents.AddAsync(content, cancellationToken);
    }

    public async Task DeleteAsync(EducationalContent content, CancellationToken cancellationToken = default)
    {
        _dbContext.EducationalContents.Remove(content);
        await Task.CompletedTask;
    }

    public async Task<bool> IsContentInUseAsync(Guid contentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LessonBlocks
            .AnyAsync(b => b.ReferencedEducationalContentId == contentId, cancellationToken);
    }

    private static IQueryable<EducationalContent> ApplyFilters(
        IQueryable<EducationalContent> query,
        string? search,
        EducationalContentType? type,
        ReferenceLevel? level)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(searchLower) || c.Summary.ToLower().Contains(searchLower));
        }

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        if (level.HasValue)
        {
            query = query.Where(c => c.ReferenceLevel == level.Value);
        }

        return query;
    }
}
