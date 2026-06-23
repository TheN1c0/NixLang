using MediatR;
using NixLang.Application.Common.Models;

namespace NixLang.Application.Lessons.Queries.GetLessons;

/// <summary>
/// Query to retrieve a paginated list of published lessons.
/// </summary>
public record GetLessonsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResult<LessonSummaryDto>>;
