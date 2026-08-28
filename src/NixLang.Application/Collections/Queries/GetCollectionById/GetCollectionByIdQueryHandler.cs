using MediatR;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Collections.Queries.GetCollectionById;

public class GetCollectionByIdQueryHandler : IRequestHandler<GetCollectionByIdQuery, CollectionDetailDto?>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCollectionByIdQueryHandler(
        ICollectionRepository collectionRepository,
        IFavoriteRepository favoriteRepository,
        ILessonProgressRepository progressRepository,
        ICurrentUserService currentUserService)
    {
        _collectionRepository = collectionRepository;
        _favoriteRepository = favoriteRepository;
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CollectionDetailDto?> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
    {
        var collection = await _collectionRepository.GetPublishedByIdAsync(request.Id, cancellationToken);
        if (collection == null)
            return null;

        var userId = _currentUserService.UserId;
        var favorites = await _favoriteRepository.GetByUserIdAsync(userId, cancellationToken);
        var progresses = await _progressRepository.GetByUserIdAsync(userId, cancellationToken);

        // Filter only published lessons for student view
        var publishedLessons = collection.CollectionLessons
            .Where(cl => cl.Lesson != null && cl.Lesson.Status == PublicationStatus.Published)
            .OrderBy(cl => cl.Order)
            .ToList();

        var lessonDtos = publishedLessons.Select(cl =>
        {
            var lesson = cl.Lesson!;
            var isFav = favorites.Any(f => f.LessonId == lesson.Id);
            var prog = progresses.FirstOrDefault(p => p.LessonId == lesson.Id);

            return new CollectionLessonItemDto(
                cl.Id,
                lesson.Id,
                lesson.Title,
                lesson.Description,
                lesson.ReferenceLevel.ToString(),
                cl.Order,
                isFav,
                prog?.ProgressPercentage ?? 0m,
                prog?.Status.ToString() ?? "NotStarted");
        }).ToList().AsReadOnly();

        var totalLessons = lessonDtos.Count;
        var completedLessons = lessonDtos.Count(l => l.Status == "Completed");
        var progressPercentage = totalLessons > 0 
            ? Math.Round((decimal)completedLessons / totalLessons * 100m, 2) 
            : 0m;

        return new CollectionDetailDto(
            collection.Id,
            collection.Title,
            collection.Description,
            collection.IconUrl,
            collection.SuggestedLevel?.ToString(),
            totalLessons,
            completedLessons,
            progressPercentage,
            lessonDtos);
    }
}
