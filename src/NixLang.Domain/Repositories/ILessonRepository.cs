using NixLang.Domain.Entities;

namespace NixLang.Domain.Repositories;

public interface ILessonRepository
{
    /// <summary>
    /// Returns a paginated list of published lessons ordered by CreatedAt descending.
    /// </summary>
    Task<IReadOnlyList<Lesson>> GetPublishedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of published lessons.
    /// </summary>
    Task<int> CountPublishedAsync(CancellationToken cancellationToken = default);
}
