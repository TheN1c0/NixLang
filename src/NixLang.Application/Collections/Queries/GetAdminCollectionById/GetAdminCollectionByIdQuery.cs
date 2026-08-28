using MediatR;

namespace NixLang.Application.Collections.Queries.GetAdminCollectionById;

public record AdminCollectionLessonItemDto(
    Guid Id,
    Guid LessonId,
    string Title,
    string ReferenceLevel,
    string Status,
    int Order);

public record AdminCollectionDetailDto(
    Guid Id,
    string Title,
    string Description,
    string? IconUrl,
    string? SuggestedLevel,
    string Status,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<AdminCollectionLessonItemDto> Lessons);

public record GetAdminCollectionByIdQuery(Guid Id) : IRequest<AdminCollectionDetailDto?>;
