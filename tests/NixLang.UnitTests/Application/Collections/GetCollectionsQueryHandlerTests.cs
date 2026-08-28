using Microsoft.Extensions.Logging;
using NSubstitute;
using NixLang.Application.Collections.Queries.GetCollections;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using Xunit;

namespace NixLang.UnitTests.Application.Collections;

public class GetCollectionsQueryHandlerTests
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetCollectionsQueryHandler> _logger;
    private readonly GetCollectionsQueryHandler _handler;

    public GetCollectionsQueryHandlerTests()
    {
        _collectionRepository = Substitute.For<ICollectionRepository>();
        _progressRepository = Substitute.For<ILessonProgressRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _logger = Substitute.For<ILogger<GetCollectionsQueryHandler>>();

        _currentUserService.UserId.Returns(Guid.NewGuid());

        _handler = new GetCollectionsQueryHandler(
            _collectionRepository,
            _progressRepository,
            _currentUserService,
            _logger);
    }

    [Fact]
    public async Task Handle_ShouldCalculateDerivedProgressAccurately()
    {
        // Arrange
        var userId = _currentUserService.UserId;
        var collection = new Collection("Travel English", "English for travelling", null, ReferenceLevel.A2);
        collection.Publish();

        var lesson1 = new Lesson("At the Airport", "Learn airport vocab", ReferenceLevel.A2);
        lesson1.Publish();
        var lesson2 = new Lesson("Hotel Check-in", "Learn hotel check-in", ReferenceLevel.A2);
        lesson2.Publish();

        collection.AddLesson(lesson1.Id);
        collection.AddLesson(lesson2.Id);

        // Populate navigation for testing
        var cl1 = collection.CollectionLessons.First(cl => cl.LessonId == lesson1.Id);
        typeof(CollectionLesson).GetProperty("Lesson")!.SetValue(cl1, lesson1);

        var cl2 = collection.CollectionLessons.First(cl => cl.LessonId == lesson2.Id);
        typeof(CollectionLesson).GetProperty("Lesson")!.SetValue(cl2, lesson2);

        _collectionRepository.CountPublishedAsync(null, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));
        _collectionRepository.GetPublishedAsync(1, 10, null, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Collection>>(new List<Collection> { collection }));

        // User completed only lesson 1
        var completedProgress = new LessonProgress(userId, lesson1.Id);
        completedProgress.UpdateProgress(100m, ProgressStatus.Completed);

        _progressRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<LessonProgress> { completedProgress }));

        var query = new GetCollectionsQuery(1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        var item = result.Items[0];
        Assert.Equal("Travel English", item.Title);
        Assert.Equal(2, item.TotalLessons);
        Assert.Equal(1, item.CompletedLessons);
        Assert.Equal(50.0m, item.ProgressPercentage);
    }
}
