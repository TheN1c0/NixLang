using NixLang.Domain.Entities;

namespace NixLang.Domain.Repositories;

public interface IExerciseRepository
{
    Task<IReadOnlyList<Exercise>> GetAllAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsExerciseUsedInLessonAsync(Guid exerciseId, CancellationToken cancellationToken = default);
    Task AddAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task DeleteAsync(Exercise exercise, CancellationToken cancellationToken = default);
    Task ClearOptionsAsync(Guid exerciseId, CancellationToken cancellationToken = default);
    Task AddOptionAsync(ExerciseOption option, CancellationToken cancellationToken = default);
}
