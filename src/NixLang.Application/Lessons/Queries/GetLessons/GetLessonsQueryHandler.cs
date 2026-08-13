using MediatR;
using NixLang.Application.Common.Models;
using NixLang.Domain.Repositories;

using NixLang.Application.Common.Interfaces;
using System.Linq;

namespace NixLang.Application.Lessons.Queries.GetLessons;

public class GetLessonsQueryHandler : IRequestHandler<GetLessonsQuery, PagedResult<LessonSummaryDto>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonsQueryHandler(
        ILessonRepository lessonRepository,
        IFavoriteRepository favoriteRepository,
        ILessonProgressRepository progressRepository,
        ICurrentUserService currentUserService)
    {
        _lessonRepository = lessonRepository;
        _favoriteRepository = favoriteRepository;
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<LessonSummaryDto>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Get total count of published lessons
        var totalCount = await _lessonRepository.CountPublishedAsync(request.Search, request.Level, request.CategoryIds, cancellationToken);

        // 2. Get paginated published lessons
        var lessons = await _lessonRepository.GetPublishedAsync(request.Page, request.PageSize, request.Search, request.Level, request.CategoryIds, cancellationToken);

        // 3. Get user specific records
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken);
        var progresses = await _progressRepository.GetByUserIdAsync(userId, cancellationToken);

        // 4. Map domain entities to DTOs
        var items = lessons
            .Select(l => {
                var isFav = favorites.Any(f => f.LessonId == l.Id);
                var prog = progresses.FirstOrDefault(p => p.LessonId == l.Id);
                return new LessonSummaryDto(
                    l.Id,
                    l.Title,
                    l.Description,
                    l.ReferenceLevel.ToString(),
                    isFav,
                    prog?.ProgressPercentage ?? 0m,
                    prog?.Status.ToString() ?? "NotStarted");
            })
            .ToList()
            .AsReadOnly();

        // 5. Return paged result with metadata
        return new PagedResult<LessonSummaryDto>(items, request.Page, request.PageSize, totalCount);
    }
}
