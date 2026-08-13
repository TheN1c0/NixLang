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

    /// <summary>
    /// Returns a paginated list of all lessons (published, draft, archived) ordered by CreatedAt descending.
    /// </summary>
    Task<IReadOnlyList<Lesson>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of all lessons.
    /// </summary>
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a lesson by its identifier (regardless of publication status), including its lesson blocks and associated exercises.
    /// </summary>
    Task<Lesson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new lesson to the persistence store.
    /// </summary>
    Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a lesson from the persistence store.
    /// </summary>
    Task DeleteAsync(Lesson lesson, CancellationToken cancellationToken = default);
    Task ClearLessonBlocksAsync(Guid lessonId, CancellationToken cancellationToken = default);
    Task AddLessonBlockAsync(LessonBlock block, CancellationToken cancellationToken = default);
}

