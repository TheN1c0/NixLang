using NixLang.Domain.Entities;

namespace NixLang.Domain.Repositories;

public interface ICollectionRepository
{
    /// <summary>
    /// Returns a paginated list of published collections ordered by DisplayOrder ascending, then CreatedAt descending.
    /// Optionally filtered by a search term in Title or Description, and suggested level.
    /// </summary>
    Task<IReadOnlyList<Collection>> GetPublishedAsync(
        int page, 
        int pageSize, 
        string? search = null, 
        string? level = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of published collections matching the given filters.
    /// </summary>
    Task<int> CountPublishedAsync(
        string? search = null, 
        string? level = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a published collection by its identifier, including its associated lessons ordered by sequence.
    /// </summary>
    Task<Collection?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of all collections (draft, published, disabled) ordered by DisplayOrder ascending, then CreatedAt descending.
    /// </summary>
    Task<IReadOnlyList<Collection>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total count of all collections.
    /// </summary>
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a collection by its identifier (regardless of publication status), including its associated lessons.
    /// </summary>
    Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new collection to the persistence store.
    /// </summary>
    Task AddAsync(Collection collection, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a collection from the persistence store.
    /// </summary>
    Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default);
}
