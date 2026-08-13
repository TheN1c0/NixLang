using MediatR;
using NixLang.Domain.Repositories;
using NixLang.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Application.Lessons.Queries.GetUserProgress;

public record UserLessonProgressDto(
    Guid LessonId,
    string LessonTitle,
    string ReferenceLevel,
    decimal ProgressPercentage,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt);

public record UserStatsDto(
    int LessonsCompleted,
    int LessonsInProgress,
    int FavoritesCount);

public record UserProgressResponseDto(
    UserStatsDto Stats,
    IReadOnlyList<UserLessonProgressDto> ProgressList,
    IReadOnlyList<Guid> FavoriteLessonIds);

public record GetUserProgressQuery : IRequest<UserProgressResponseDto>;

public class GetUserProgressQueryHandler : IRequestHandler<GetUserProgressQuery, UserProgressResponseDto>
{
    private readonly ILessonProgressRepository _progressRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserProgressQueryHandler(
        ILessonProgressRepository progressRepository,
        IFavoriteRepository favoriteRepository,
        ILessonRepository lessonRepository,
        ICurrentUserService currentUserService)
    {
        _progressRepository = progressRepository;
        _favoriteRepository = favoriteRepository;
        _lessonRepository = lessonRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UserProgressResponseDto> Handle(GetUserProgressQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Get user records from repositories
        var progressList = await _progressRepository.GetByUserIdAsync(userId, cancellationToken);
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken);

        // 2. Fetch all lesson details in a batch query to map titles/levels
        // Let's get the list of lesson IDs involved
        var lessonIds = progressList.Select(p => p.LessonId)
            .Concat(favorites.Select(f => f.LessonId))
            .Distinct()
            .ToList();

        var lessons = new List<Domain.Entities.Lesson>();
        foreach (var id in lessonIds)
        {
            var l = await _lessonRepository.GetPublishedByIdAsync(id, cancellationToken);
            if (l != null)
            {
                lessons.Add(l);
            }
        }

        // 3. Map details
        var progressDtos = progressList
            .Select(p => {
                var lesson = lessons.FirstOrDefault(l => l.Id == p.LessonId);
                return new UserLessonProgressDto(
                    p.LessonId,
                    lesson?.Title ?? "Unknown Lesson",
                    lesson?.ReferenceLevel.ToString() ?? "A1",
                    p.ProgressPercentage,
                    p.Status.ToString(),
                    p.StartedAt,
                    p.CompletedAt);
            })
            .ToList()
            .AsReadOnly();

        // 4. Compute statistics
        var completedCount = progressList.Count(p => p.Status == Domain.Enums.ProgressStatus.Completed);
        var inProgressCount = progressList.Count(p => p.Status == Domain.Enums.ProgressStatus.InProgress);
        var favIds = favorites.Select(f => f.LessonId).ToList().AsReadOnly();

        var stats = new UserStatsDto(completedCount, inProgressCount, favIds.Count);

        return new UserProgressResponseDto(stats, progressDtos, favIds);
    }
}
