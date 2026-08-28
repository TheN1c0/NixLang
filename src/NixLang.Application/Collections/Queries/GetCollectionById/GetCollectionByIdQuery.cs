using MediatR;

namespace NixLang.Application.Collections.Queries.GetCollectionById;

public record CollectionLessonItemDto(
    Guid Id,
    Guid LessonId,
    string Title,
    string Description,
    string ReferenceLevel,
    int Order,
    bool IsFavorite,
    decimal ProgressPercentage,
    string Status);

public record CollectionDetailDto(
    Guid Id,
    string Title,
    string Description,
    string? IconUrl,
    string? SuggestedLevel,
    int TotalLessons,
    int CompletedLessons,
    decimal ProgressPercentage,
    IReadOnlyList<CollectionLessonItemDto> Lessons);

public record GetCollectionByIdQuery(Guid Id) : IRequest<CollectionDetailDto?>;
