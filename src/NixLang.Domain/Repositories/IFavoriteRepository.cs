using NixLang.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Domain.Repositories;

public interface IFavoriteRepository
{
    Task<Favorite?> GetAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default);
    Task<List<Favorite>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
    void Remove(Favorite favorite);
    Task<bool> ExistsAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default);
}
