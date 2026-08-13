using MediatR;
using NixLang.Application.Common.Models;

namespace NixLang.Application.Lessons.Queries.GetAdminLessons;

public record AdminLessonSummaryDto(
    Guid Id,
    string Title,
    string Description,
    string ReferenceLevel,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetAdminLessonsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<AdminLessonSummaryDto>>;
