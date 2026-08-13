using MediatR;
using NixLang.Application.Common.Models;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Lessons.Queries.GetAdminLessons;

public class GetAdminLessonsQueryHandler : IRequestHandler<GetAdminLessonsQuery, PagedResult<AdminLessonSummaryDto>>
{
    private readonly ILessonRepository _lessonRepository;

    public GetAdminLessonsQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<PagedResult<AdminLessonSummaryDto>> Handle(GetAdminLessonsQuery request, CancellationToken cancellationToken)
    {
        var lessons = await _lessonRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var totalCount = await _lessonRepository.CountAllAsync(cancellationToken);

        var dtos = lessons.Select(l => new AdminLessonSummaryDto(
            l.Id,
            l.Title,
            l.Description,
            l.ReferenceLevel.ToString(),
            l.Status.ToString(),
            l.CreatedAt,
            l.UpdatedAt
        )).ToList();

        return new PagedResult<AdminLessonSummaryDto>(dtos, request.Page, request.PageSize, totalCount);
    }
}
