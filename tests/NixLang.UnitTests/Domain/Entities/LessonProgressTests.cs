using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using System;
using Xunit;

namespace NixLang.UnitTests.Domain.Entities;

public class LessonProgressTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _lessonId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidArguments_InitializesCorrectly()
    {
        // Act
        var progress = new LessonProgress(_userId, _lessonId);

        // Assert
        Assert.Equal(_userId, progress.UserId);
        Assert.Equal(_lessonId, progress.LessonId);
        Assert.Equal(ProgressStatus.NotStarted, progress.Status);
        Assert.Equal(0m, progress.ProgressPercentage);
        Assert.Null(progress.CompletedAt);
        Assert.Empty(progress.ExerciseResults);
    }

    [Fact]
    public void UpdateProgress_FirstExecution_PartialProgress_SetsInProgress()
    {
        // Arrange
        var progress = new LessonProgress(_userId, _lessonId);

        // Act
        progress.UpdateProgress(25m, ProgressStatus.InProgress);

        // Assert
        Assert.Equal(ProgressStatus.InProgress, progress.Status);
        Assert.Equal(25m, progress.ProgressPercentage);
        Assert.Null(progress.CompletedAt);
    }

    [Fact]
    public void UpdateProgress_Reaching100Percent_SetsCompletedAndCompletedAt()
    {
        // Arrange
        var progress = new LessonProgress(_userId, _lessonId);
        progress.UpdateProgress(50m, ProgressStatus.InProgress);

        // Act
        progress.UpdateProgress(100m, ProgressStatus.Completed);

        // Assert
        Assert.Equal(ProgressStatus.Completed, progress.Status);
        Assert.Equal(100m, progress.ProgressPercentage);
        Assert.NotNull(progress.CompletedAt);
    }

    [Fact]
    public void UpdateProgress_AlreadyCompleted_ReceivingPartialProgress_PreservesCompletedAnd100Percent()
    {
        // Arrange — Lesson was completed
        var progress = new LessonProgress(_userId, _lessonId);
        progress.UpdateProgress(100m, ProgressStatus.Completed);
        var originalCompletedAt = progress.CompletedAt;
        Assert.NotNull(originalCompletedAt);

        // Act — User repeats lesson and reaches 25% (partial)
        progress.UpdateProgress(25m, ProgressStatus.InProgress);

        // Assert — Status, percentage and CompletedAt must NOT be degraded
        Assert.Equal(ProgressStatus.Completed, progress.Status);
        Assert.Equal(100m, progress.ProgressPercentage);
        Assert.Equal(originalCompletedAt, progress.CompletedAt);
    }

    [Fact]
    public void UpdateProgress_AlreadyCompleted_AbandoningRepetition_PreservesCompletedStatus()
    {
        // Arrange — Completed lesson
        var progress = new LessonProgress(_userId, _lessonId);
        progress.UpdateProgress(100m, ProgressStatus.Completed);
        var originalCompletedAt = progress.CompletedAt;

        // Act — Multiple partial updates during a repeat session that gets abandoned
        progress.UpdateProgress(14m, ProgressStatus.InProgress);
        progress.UpdateProgress(28m, ProgressStatus.InProgress);

        // Assert
        Assert.Equal(ProgressStatus.Completed, progress.Status);
        Assert.Equal(100m, progress.ProgressPercentage);
        Assert.Equal(originalCompletedAt, progress.CompletedAt);
    }

    [Fact]
    public void UpdateProgress_AlreadyCompleted_CompletingAgain_PreservesCompletedStatus()
    {
        // Arrange
        var progress = new LessonProgress(_userId, _lessonId);
        progress.UpdateProgress(100m, ProgressStatus.Completed);
        var originalCompletedAt = progress.CompletedAt;

        // Act — User finishes repeat session at 100%
        progress.UpdateProgress(100m, ProgressStatus.Completed);

        // Assert
        Assert.Equal(ProgressStatus.Completed, progress.Status);
        Assert.Equal(100m, progress.ProgressPercentage);
        Assert.Equal(originalCompletedAt, progress.CompletedAt);
    }

    [Fact]
    public void UpdateProgress_RepeatingMultipleTimes_NeverDegradesStatus()
    {
        // Arrange
        var progress = new LessonProgress(_userId, _lessonId);
        progress.UpdateProgress(100m, ProgressStatus.Completed);

        // Act & Assert — Repetition 1
        progress.UpdateProgress(30m, ProgressStatus.InProgress);
        Assert.Equal(ProgressStatus.Completed, progress.Status);
        Assert.Equal(100m, progress.ProgressPercentage);

        // Repetition 2
        progress.UpdateProgress(70m, ProgressStatus.InProgress);
        Assert.Equal(ProgressStatus.Completed, progress.Status);
        Assert.Equal(100m, progress.ProgressPercentage);

        // Repetition 3 completion
        progress.UpdateProgress(100m, ProgressStatus.Completed);
        Assert.Equal(ProgressStatus.Completed, progress.Status);
        Assert.Equal(100m, progress.ProgressPercentage);
    }

    [Fact]
    public void UpdateProgress_InProgress_LowerPercentageReceived_RetainsHighestPercentage()
    {
        // Arrange
        var progress = new LessonProgress(_userId, _lessonId);
        progress.UpdateProgress(60m, ProgressStatus.InProgress);

        // Act — received lower percentage (e.g. user went back a step)
        progress.UpdateProgress(40m, ProgressStatus.InProgress);

        // Assert — retains highest achieved percentage
        Assert.Equal(60m, progress.ProgressPercentage);
        Assert.Equal(ProgressStatus.InProgress, progress.Status);
    }

    [Fact]
    public void AddExerciseResult_UpdatesExerciseResultsEvenWhenLessonIsCompleted()
    {
        // Arrange
        var progress = new LessonProgress(_userId, _lessonId);
        progress.UpdateProgress(100m, ProgressStatus.Completed);

        var exerciseId = Guid.NewGuid();

        // Act — Repeat lesson and answer exercise
        progress.AddExerciseResult(exerciseId, "Answer 1", true);

        // Assert
        Assert.Single(progress.ExerciseResults);
        var result = Assert.Single(progress.ExerciseResults);
        Assert.Equal(exerciseId, result.ExerciseId);
        Assert.Equal("Answer 1", result.GivenAnswer);
        Assert.True(result.IsCorrect);

        // Update with new answer on subsequent attempt
        progress.AddExerciseResult(exerciseId, "Answer 2", true);
        Assert.Single(progress.ExerciseResults);
        Assert.Equal("Answer 2", progress.ExerciseResults.First().GivenAnswer);
    }
}
