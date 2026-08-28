using MediatR;
using Microsoft.Extensions.Logging;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Common.Models;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Collections.Queries.GetCollections;

public class GetCollectionsQueryHandler : IRequestHandler<GetCollectionsQuery, PagedResult<CollectionSummaryDto>>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCollectionsQueryHandler> _logger;

    public GetCollectionsQueryHandler(
        ICollectionRepository collectionRepository,
        ILessonProgressRepository progressRepository,
        ICurrentUserService currentUserService,
        ILogger<GetCollectionsQueryHandler> logger)
    {
        _collectionRepository = collectionRepository;
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PagedResult<CollectionSummaryDto>> Handle(GetCollectionsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Get total published collections count
        var totalCount = await _collectionRepository.CountPublishedAsync(request.Search, request.Level, cancellationToken);

        // 2. Get paginated published collections with lessons included
        var collections = await _collectionRepository.GetPublishedAsync(request.Page, request.PageSize, request.Search, request.Level, cancellationToken);

        // 3. Get user progress records
        var progresses = await _progressRepository.GetByUserIdAsync(userId, cancellationToken);
        var completedLessonIds = progresses
            .Where(p => p.Status == ProgressStatus.Completed)
            .Select(p => p.LessonId)
            .ToHashSet();

        // 4. Map to DTOs calculating derived progress
        var items = collections.Select(c =>
        {
            // Only consider published lessons in the collection for student
            var publishedLessons = c.CollectionLessons
                .Where(cl => cl.Lesson != null && cl.Lesson.Status == PublicationStatus.Published)
                .ToList();

            var totalLessons = publishedLessons.Count;
            var completedLessons = publishedLessons.Count(cl => completedLessonIds.Contains(cl.LessonId));
            var progressPercentage = totalLessons > 0 
                ? Math.Round((decimal)completedLessons / totalLessons * 100m, 2) 
                : 0m;

            return new CollectionSummaryDto(
                c.Id,
                c.Title,
                c.Description,
                c.IconUrl,
                c.SuggestedLevel?.ToString(),
                c.DisplayOrder,
                totalLessons,
                completedLessons,
                progressPercentage);
        }).ToList().AsReadOnly();

        return new PagedResult<CollectionSummaryDto>(items, request.Page, request.PageSize, totalCount);
    }
}
