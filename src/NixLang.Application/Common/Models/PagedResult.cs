namespace NixLang.Application.Common.Models;

/// <summary>
/// Generic wrapper for paginated query results.
/// Reusable across any endpoint that requires pagination.
/// </summary>
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
