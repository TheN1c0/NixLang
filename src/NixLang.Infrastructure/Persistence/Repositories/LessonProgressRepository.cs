using Microsoft.EntityFrameworkCore;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Infrastructure.Persistence.Repositories;

public class LessonProgressRepository : ILessonProgressRepository
{
    private readonly NixLangDbContext _dbContext;

    public LessonProgressRepository(NixLangDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LessonProgress?> GetAsync(Guid userId, Guid lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LessonProgress
            .Include(p => p.ExerciseResults)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, cancellationToken);
    }

    public async Task<List<LessonProgress>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LessonProgress
            .Include(p => p.ExerciseResults)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LessonProgress progress, CancellationToken cancellationToken = default)
    {
        await _dbContext.LessonProgress.AddAsync(progress, cancellationToken);
    }
}
