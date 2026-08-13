using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Exercises.Commands.DeleteExercise;
using NixLang.Domain.Common;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using Xunit;

namespace NixLang.UnitTests.Application.Exercises;

public class DeleteExerciseCommandHandlerTests
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteExerciseCommandHandler _handler;

    public DeleteExerciseCommandHandlerTests()
    {
        _exerciseRepository = Substitute.For<IExerciseRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteExerciseCommandHandler(_exerciseRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithUnusedExercise_ShouldDeleteSuccessfully()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = new Exercise(ExerciseType.Translation, "Hello", "Hola");
        // Use reflection to set private Id if needed, or check if Id matches
        typeof(BaseEntity).GetProperty("Id")?.SetValue(exercise, exerciseId);

        _exerciseRepository.GetByIdAsync(exerciseId, Arg.Any<CancellationToken>()).Returns(exercise);
        _exerciseRepository.IsExerciseUsedInLessonAsync(exerciseId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.Handle(new DeleteExerciseCommand(exerciseId), CancellationToken.None);

        // Assert
        Assert.True(result);
        await _exerciseRepository.Received(1).DeleteAsync(exercise, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExerciseUsedInLesson_ShouldThrowExerciseInUseException()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = new Exercise(ExerciseType.Translation, "Hello", "Hola");
        typeof(BaseEntity).GetProperty("Id")?.SetValue(exercise, exerciseId);

        _exerciseRepository.GetByIdAsync(exerciseId, Arg.Any<CancellationToken>()).Returns(exercise);
        _exerciseRepository.IsExerciseUsedInLessonAsync(exerciseId, Arg.Any<CancellationToken>()).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<ExerciseInUseException>(() =>
            _handler.Handle(new DeleteExerciseCommand(exerciseId), CancellationToken.None));

        await _exerciseRepository.DidNotReceive().DeleteAsync(Arg.Any<Exercise>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingExercise_ShouldThrowExerciseNotFoundException()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        _exerciseRepository.GetByIdAsync(exerciseId, Arg.Any<CancellationToken>()).Returns((Exercise?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ExerciseNotFoundException>(() =>
            _handler.Handle(new DeleteExerciseCommand(exerciseId), CancellationToken.None));
    }
}
