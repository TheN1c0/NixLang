using MediatR;
using NixLang.Application.Common.Models;

namespace NixLang.Application.Collections.Queries.GetCollections;

public record CollectionSummaryDto(
    Guid Id,
    string Title,
    string Description,
    string? IconUrl,
    string? SuggestedLevel,
    int DisplayOrder,
    int TotalLessons,
    int CompletedLessons,
    decimal ProgressPercentage);

public record GetCollectionsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Level = null) : IRequest<PagedResult<CollectionSummaryDto>>;
