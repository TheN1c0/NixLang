using NixLang.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Domain.Repositories;

public interface ILessonProgressRepository
{
    Task<LessonProgress?> GetAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default);
    Task<List<LessonProgress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(LessonProgress progress, CancellationToken cancellationToken = default);
}
