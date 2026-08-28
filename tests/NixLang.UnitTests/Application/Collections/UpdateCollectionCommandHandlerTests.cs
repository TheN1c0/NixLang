using NSubstitute;
using NixLang.Application.Collections.Commands.UpdateCollection;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using Xunit;

namespace NixLang.UnitTests.Application.Collections;

public class UpdateCollectionCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateCollectionCommandHandler _handler;

    public UpdateCollectionCommandHandlerTests()
    {
        _collectionRepository = Substitute.For<ICollectionRepository>();
        _lessonRepository = Substitute.For<ILessonRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new UpdateCollectionCommandHandler(
            _collectionRepository,
            _lessonRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_PublishingWithoutPublishedLessons_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var collection = new Collection("Title", "Desc");
        _collectionRepository.GetByIdAsync(collection.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Collection?>(collection));

        var command = new UpdateCollectionCommand(
            collection.Id,
            "Title",
            "Desc",
            null,
            "A1",
            "Published",
            0,
            new List<Guid>());

        // Act & Assert (RN-40)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Cannot publish a collection without at least one published lesson", ex.Message);
    }

    [Fact]
    public async Task Handle_PublishingWithPublishedLesson_ShouldSucceed()
    {
        // Arrange
        var collection = new Collection("Title", "Desc");
        var lesson = new Lesson("Lesson Title", "Lesson Desc", ReferenceLevel.A1);
        lesson.Publish();

        _collectionRepository.GetByIdAsync(collection.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Collection?>(collection));
        _lessonRepository.GetByIdAsync(lesson.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Lesson?>(lesson));

        var command = new UpdateCollectionCommand(
            collection.Id,
            "Updated Title",
            "Updated Desc",
            null,
            "A1",
            "Published",
            1,
            new List<Guid> { lesson.Id });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(PublicationStatus.Published, collection.Status);
        Assert.Equal("Updated Title", collection.Title);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
