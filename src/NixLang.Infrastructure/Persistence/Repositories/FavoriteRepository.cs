using Microsoft.EntityFrameworkCore;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Infrastructure.Persistence.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly NixLangDbContext _dbContext;

    public FavoriteRepository(NixLangDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Favorite?> GetAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.LessonId == lessonId, cancellationToken);
    }

    public async Task<List<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Favorites
            .Where(f => f.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
    {
        await _dbContext.Favorites.AddAsync(favorite, cancellationToken);
    }

    public void Remove(Favorite favorite)
    {
        _dbContext.Favorites.Remove(favorite);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Favorites
            .AnyAsync(f => f.UserId == userId && f.LessonId == lessonId, cancellationToken);
    }
}
