using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Lessons.Queries.GetLessonById;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using Xunit;

namespace NixLang.UnitTests.Application.Lessons;

public class GetLessonByIdQueryHandlerTests
{
    private readonly ILessonRepository _lessonRepository;
    private readonly GetLessonByIdQueryHandler _handler;

    public GetLessonByIdQueryHandlerTests()
    {
        _lessonRepository = Substitute.For<ILessonRepository>();
        _handler = new GetLessonByIdQueryHandler(_lessonRepository);
    }

    [Fact]
    public async Task Handle_WithExistingPublishedLesson_ShouldReturnLessonDetailDto()
    {
        // Arrange
        var lesson = new Lesson("Present Simple", "Learn the present simple tense", ReferenceLevel.A1);
        var lessonId = lesson.Id; // Already generated upon construction
        lesson.Publish();

        // Add 2 mock exercises via blocks
        var ex1 = new Exercise(ExerciseType.MultipleChoice, "Question 1");
        var ex2 = new Exercise(ExerciseType.Translation, "Question 2");
        var block1 = LessonBlock.CreateExerciseBlock(lessonId, 1, ex1.Id);
        var block2 = LessonBlock.CreateExerciseBlock(lessonId, 2, ex2.Id);

        AddLessonBlockWithExercise(lesson, block1, ex1);
        AddLessonBlockWithExercise(lesson, block2, ex2);

        _lessonRepository.GetPublishedByIdAsync(lessonId, Arg.Any<CancellationToken>()).Returns(lesson);

        // Act
        var result = await _handler.Handle(new GetLessonByIdQuery(lessonId), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lessonId, result.Id);
        Assert.Equal("Present Simple", result.Title);
        Assert.Equal("Learn the present simple tense", result.Description);
        Assert.Equal("A1", result.ReferenceLevel);
        Assert.Equal(2, result.ExerciseCount);
        Assert.Equal(2, result.LessonBlocks.Count);
    }

    [Fact]
    public async Task Handle_WithNonExistingLesson_ShouldThrowLessonNotFoundException()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        _lessonRepository.GetPublishedByIdAsync(lessonId, Arg.Any<CancellationToken>()).Returns((Lesson?)null);

        // Act & Assert
        await Assert.ThrowsAsync<LessonNotFoundException>(() =>
            _handler.Handle(new GetLessonByIdQuery(lessonId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectId()
    {
        // Arrange
        var lesson = new Lesson("Dummy", "Dummy", ReferenceLevel.B1);
        var lessonId = lesson.Id;
        lesson.Publish();
        _lessonRepository.GetPublishedByIdAsync(lessonId, Arg.Any<CancellationToken>()).Returns(lesson);

        // Act
        await _handler.Handle(new GetLessonByIdQuery(lessonId), CancellationToken.None);

        // Assert
        await _lessonRepository.Received(1).GetPublishedByIdAsync(lessonId, Arg.Any<CancellationToken>());
    }

    private static void AddLessonBlockWithExercise(Lesson lesson, LessonBlock block, Exercise exercise)
    {
        typeof(LessonBlock).GetProperty("Exercise")?.SetValue(block, exercise);
        lesson.AddLessonBlock(block);
    }
}
