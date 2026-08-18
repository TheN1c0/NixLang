using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<GetLessonsQueryHandler> _logger;

    public GetLessonsQueryHandler(
        ILessonRepository lessonRepository,
        IFavoriteRepository favoriteRepository,
        ILessonProgressRepository progressRepository,
        ICurrentUserService currentUserService,
        ILogger<GetLessonsQueryHandler> logger)
    {
        _lessonRepository = lessonRepository;
        _favoriteRepository = favoriteRepository;
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<LessonSummaryDto>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        _logger.LogWarning("[DIAG-GetLessons] UserId: {UserId}", userId);

        // 1. Get total count of published lessons
        var totalCount = await _lessonRepository.CountPublishedAsync(request.Search, request.Level, request.CategoryIds, cancellationToken);

        // 2. Get paginated published lessons
        var lessons = await _lessonRepository.GetPublishedAsync(request.Page, request.PageSize, request.Search, request.Level, request.CategoryIds, cancellationToken);

        // 3. Get user specific records
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken);
        var progresses = await _progressRepository.GetByUserIdAsync(userId, cancellationToken);

        _logger.LogWarning("[DIAG-GetLessons] Found {Count} progress records for user {UserId}", progresses.Count, userId);
        foreach (var p in progresses)
        {
            _logger.LogWarning("[DIAG-GetLessons]   LessonId={LessonId}, Status={Status}, Percentage={Pct}%, CompletedAt={CompletedAt}",
                p.LessonId, p.Status, p.ProgressPercentage, p.CompletedAt);
        }

        // 4. Map domain entities to DTOs
        var items = lessons
            .Select(l => {
                var isFav = favorites.Any(f => f.LessonId == l.Id);
                var prog = progresses.FirstOrDefault(p => p.LessonId == l.Id);
                var dto = new LessonSummaryDto(
                    l.Id,
                    l.Title,
                    l.Description,
                    l.ReferenceLevel.ToString(),
                    isFav,
                    prog?.ProgressPercentage ?? 0m,
                    prog?.Status.ToString() ?? "NotStarted");
                _logger.LogWarning("[DIAG-GetLessons]   DTO: Lesson={Title}, ProgressPct={Pct}, Status={Status}, HasProgressRecord={HasProg}",
                    l.Title, dto.ProgressPercentage, dto.Status, prog != null);
                return dto;
            })
            .ToList()
            .AsReadOnly();

        // 5. Return paged result with metadata
        return new PagedResult<LessonSummaryDto>(items, request.Page, request.PageSize, totalCount);
    }
}
