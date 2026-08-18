using NixLang.Domain.Common;

namespace NixLang.Domain.Entities;

/// <summary>
/// Records the result of a user's answer to a specific exercise within a lesson attempt.
/// Belongs to the Progress Aggregate.
/// Maps to: ResultadoEjercicio.
/// Source: RN-15, RN-16, RN-28.
/// </summary>
public class ExerciseResult : BaseEntity
{
    public Guid LessonProgressId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string GivenAnswer { get; private set; }
    public bool IsCorrect { get; private set; }
    public DateTime AnsweredAt { get; private set; }

    protected ExerciseResult() : base()
    {
        GivenAnswer = string.Empty;
    }

    public ExerciseResult(Guid lessonProgressId, Guid exerciseId, string givenAnswer, bool isCorrect)
        : base()
    {
        if (lessonProgressId == Guid.Empty)
            throw new ArgumentException("Lesson progress ID cannot be empty.", nameof(lessonProgressId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("Exercise ID cannot be empty.", nameof(exerciseId));

        LessonProgressId = lessonProgressId;
        ExerciseId = exerciseId;
        GivenAnswer = givenAnswer ?? string.Empty;
        IsCorrect = isCorrect;
        AnsweredAt = DateTime.UtcNow;
    }

    public void Update(string givenAnswer, bool isCorrect)
    {
        GivenAnswer = givenAnswer ?? string.Empty;
        IsCorrect = isCorrect;
        AnsweredAt = DateTime.UtcNow;
    }
}

