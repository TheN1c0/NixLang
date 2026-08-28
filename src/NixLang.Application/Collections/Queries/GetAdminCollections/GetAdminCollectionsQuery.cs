using MediatR;
using NixLang.Application.Common.Models;

namespace NixLang.Application.Collections.Queries.GetAdminCollections;

public record AdminCollectionSummaryDto(
    Guid Id,
    string Title,
    string Description,
    string? IconUrl,
    string? SuggestedLevel,
    string Status,
    int DisplayOrder,
    int TotalLessons,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetAdminCollectionsQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<AdminCollectionSummaryDto>>;
