using NixLang.Domain.Common;
using NixLang.Domain.Enums;

namespace NixLang.Domain.Entities;

/// <summary>
/// Tracks a user's progress in a specific lesson attempt.
/// Aggregate Root of the Progress Aggregate.
/// Maps to: ProgresoLección.
/// Source: RN-22, RN-23, RN-24, RN-25, RN-26.
/// </summary>
public class LessonProgress : BaseEntity
{
    private readonly List<ExerciseResult> _exerciseResults = [];

    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public ProgressStatus Status { get; private set; }
    public decimal ProgressPercentage { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyCollection<ExerciseResult> ExerciseResults => _exerciseResults.AsReadOnly();

    protected LessonProgress() : base() { }

    public LessonProgress(Guid userId, Guid lessonId)
        : base()
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (lessonId == Guid.Empty)
            throw new ArgumentException("Lesson ID cannot be empty.", nameof(lessonId));

        UserId = userId;
        LessonId = lessonId;
        Status = ProgressStatus.NotStarted;
        ProgressPercentage = 0m;
        StartedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(decimal progressPercentage, ProgressStatus status)
    {
        ProgressPercentage = progressPercentage;
        Status = status;
        if (status == ProgressStatus.Completed && CompletedAt == null)
        {
            CompletedAt = DateTime.UtcNow;
        }
        else if (status != ProgressStatus.Completed)
        {
            CompletedAt = null;
        }
    }

    public void AddExerciseResult(Guid exerciseId, string givenAnswer, bool isCorrect)
    {
        var existing = _exerciseResults.FirstOrDefault(r => r.ExerciseId == exerciseId);
        if (existing != null)
        {
            existing.Update(givenAnswer, isCorrect);
        }
        else
        {
            _exerciseResults.Add(new ExerciseResult(Id, exerciseId, givenAnswer, isCorrect));
        }
    }
}

