using NSubstitute;
using NixLang.Application.Common.Models;
using NixLang.Application.Lessons.Queries.GetLessons;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.UnitTests.Application.Lessons;

public class GetLessonsQueryHandlerTests
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly GetLessonsQueryHandler _handler;

    public GetLessonsQueryHandlerTests()
    {
        _lessonRepository = Substitute.For<ILessonRepository>();
        _favoriteRepository = Substitute.For<IFavoriteRepository>();
        _progressRepository = Substitute.For<ILessonProgressRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();

        _currentUserService.UserId.Returns(Guid.NewGuid());
        _favoriteRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Favorite>()));
        _progressRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<LessonProgress>()));

        _handler = new GetLessonsQueryHandler(
            _lessonRepository,
            _favoriteRepository,
            _progressRepository,
            _currentUserService,
            Substitute.For<Microsoft.Extensions.Logging.ILogger<GetLessonsQueryHandler>>());
    }


    [Fact]
    public async Task Handle_WithPublishedLessons_ShouldReturnPagedResult()
    {
        // Arrange
        var lessons = CreatePublishedLessons(3);

        _lessonRepository.CountPublishedAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(3);
        _lessonRepository.GetPublishedAsync(1, 10, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetLessonsQuery(1, 10), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task Handle_WithNoLessons_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        _lessonRepository.CountPublishedAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(0);
        _lessonRepository.GetPublishedAsync(1, 10, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Lesson>().AsReadOnly());

        // Act
        var result = await _handler.Handle(new GetLessonsQuery(1, 10), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectPaginationParams()
    {
        // Arrange
        _lessonRepository.CountPublishedAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(0);
        _lessonRepository.GetPublishedAsync(3, 20, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Lesson>().AsReadOnly());

        // Act
        await _handler.Handle(new GetLessonsQuery(3, 20), CancellationToken.None);

        // Assert
        await _lessonRepository.Received(1).GetPublishedAsync(3, 20, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
        await _lessonRepository.Received(1).CountPublishedAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapLessonToDto()
    {
        // Arrange
        var lesson = new Lesson("Present Simple", "Learn the present simple tense", ReferenceLevel.A1);
        lesson.Publish();
        var lessons = new List<Lesson> { lesson }.AsReadOnly();

        _lessonRepository.CountPublishedAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(1);
        _lessonRepository.GetPublishedAsync(1, 10, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetLessonsQuery(1, 10), CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Items);
        Assert.Equal(lesson.Id, dto.Id);
        Assert.Equal("Present Simple", dto.Title);
        Assert.Equal("Learn the present simple tense", dto.Description);
        Assert.Equal("A1", dto.ReferenceLevel);
    }

    [Theory]
    [InlineData(25, 10, 3)]
    [InlineData(10, 10, 1)]
    [InlineData(0, 10, 0)]
    [InlineData(11, 10, 2)]
    [InlineData(1, 50, 1)]
    public void PagedResult_TotalPages_CalculatesCorrectly(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange & Act
        var result = new PagedResult<LessonSummaryDto>(
            Array.Empty<LessonSummaryDto>().AsReadOnly(),
            1,
            pageSize,
            totalCount);

        // Assert
        Assert.Equal(expectedTotalPages, result.TotalPages);
    }
    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchTermToRepository()
    {
        // Arrange
        var search = "Present";
        var lessons = CreatePublishedLessons(1);

        _lessonRepository.CountPublishedAsync(search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(1);
        _lessonRepository.GetPublishedAsync(1, 10, search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetLessonsQuery(1, 10, search), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        await _lessonRepository.Received(1).GetPublishedAsync(1, 10, search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
        await _lessonRepository.Received(1).CountPublishedAsync(search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptySearchTerm_ShouldPassNullOrEmptyToRepository()
    {
        // Arrange
        var search = "";
        var lessons = CreatePublishedLessons(1);

        _lessonRepository.CountPublishedAsync(search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(1);
        _lessonRepository.GetPublishedAsync(1, 10, search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetLessonsQuery(1, 10, search), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        await _lessonRepository.Received(1).GetPublishedAsync(1, 10, search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
        await _lessonRepository.Received(1).CountPublishedAsync(search, Arg.Any<string?>(), Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithLevelFilter_ShouldPassLevelToRepository()
    {
        // Arrange
        var level = "A2";
        var lessons = CreatePublishedLessons(1);

        _lessonRepository.CountPublishedAsync(Arg.Any<string?>(), level, Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(1);
        _lessonRepository.GetPublishedAsync(1, 10, Arg.Any<string?>(), level, Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>()).Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetLessonsQuery(1, 10, null, level, null), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        await _lessonRepository.Received(1).GetPublishedAsync(1, 10, Arg.Any<string?>(), level, Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
        await _lessonRepository.Received(1).CountPublishedAsync(Arg.Any<string?>(), level, Arg.Any<IEnumerable<Guid>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCategoryIdsFilter_ShouldPassCategoryIdsToRepository()
    {
        // Arrange
        var categoryIds = new List<Guid> { Guid.NewGuid() };
        var lessons = CreatePublishedLessons(1);

        _lessonRepository.CountPublishedAsync(Arg.Any<string?>(), Arg.Any<string?>(), categoryIds, Arg.Any<CancellationToken>()).Returns(1);
        _lessonRepository.GetPublishedAsync(1, 10, Arg.Any<string?>(), Arg.Any<string?>(), categoryIds, Arg.Any<CancellationToken>()).Returns(lessons);

        // Act
        var result = await _handler.Handle(new GetLessonsQuery(1, 10, null, null, categoryIds), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        await _lessonRepository.Received(1).GetPublishedAsync(1, 10, Arg.Any<string?>(), Arg.Any<string?>(), categoryIds, Arg.Any<CancellationToken>());
        await _lessonRepository.Received(1).CountPublishedAsync(Arg.Any<string?>(), Arg.Any<string?>(), categoryIds, Arg.Any<CancellationToken>());
    }

    private static IReadOnlyList<Lesson> CreatePublishedLessons(int count)
    {
        var lessons = new List<Lesson>();
        for (int i = 1; i <= count; i++)
        {
            var lesson = new Lesson($"Lesson {i}", $"Description for lesson {i}", ReferenceLevel.A1);
            lesson.Publish();
            lessons.Add(lesson);
        }
        return lessons.AsReadOnly();
    }
}
