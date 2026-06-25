using MediatR;
using NixLang.Application.Common.Models;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Lessons.Queries.GetLessons;

public class GetLessonsQueryHandler : IRequestHandler<GetLessonsQuery, PagedResult<LessonSummaryDto>>
{
    private readonly ILessonRepository _lessonRepository;

    public GetLessonsQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<PagedResult<LessonSummaryDto>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
    {
        // 1. Get total count of published lessons
        var totalCount = await _lessonRepository.CountPublishedAsync(request.Search, request.Level, request.CategoryIds, cancellationToken);

        // 2. Get paginated published lessons
        var lessons = await _lessonRepository.GetPublishedAsync(request.Page, request.PageSize, request.Search, request.Level, request.CategoryIds, cancellationToken);

        // 3. Map domain entities to DTOs
        var items = lessons
            .Select(l => new LessonSummaryDto(
                l.Id,
                l.Title,
                l.Description,
                l.ReferenceLevel.ToString()))
            .ToList()
            .AsReadOnly();

        // 4. Return paged result with metadata
        return new PagedResult<LessonSummaryDto>(items, request.Page, request.PageSize, totalCount);
    }
}
