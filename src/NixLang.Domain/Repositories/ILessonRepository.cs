using NixLang.Domain.Entities;

namespace NixLang.Domain.Repositories;

public interface ILessonRepository
{
    /// <summary>
    /// Returns a paginated list of published lessons ordered by CreatedAt descending.
    /// Optionally filtered by a search term in Title or Description, level, and category ids.
    /// </summary>
    Task<IReadOnlyList<Lesson>> GetPublishedAsync(
        int page, 
        int pageSize, 
        string? search = null, 
        string? level = null, 
        IEnumerable<Guid>? categoryIds = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of published lessons.
    /// Optionally filtered by a search term in Title or Description, level, and category ids.
    /// </summary>
    Task<int> CountPublishedAsync(
        string? search = null, 
        string? level = null, 
        IEnumerable<Guid>? categoryIds = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a published lesson by its identifier, including its lesson blocks and associated exercises.
    /// </summary>
    Task<Lesson?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
