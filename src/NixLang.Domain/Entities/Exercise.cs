using NixLang.Domain.Common;
using NixLang.Domain.Enums;

namespace NixLang.Domain.Entities;

/// <summary>
/// Individual activity within a lesson.
/// Belongs to the Lesson Aggregate.
/// Maps to: Ejercicio.
/// Source: RN-12, RN-13, RN-14, RN-18.
/// </summary>
public class Exercise : BaseEntity
{
    public Guid LessonId { get; private set; }
    public ExerciseType Type { get; private set; }
    public string Statement { get; private set; }
    public string? CorrectAnswer { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? AudioResourceUrl { get; private set; }

    protected Exercise() : base()
    {
        Statement = string.Empty;
    }

    public Exercise(
        Guid lessonId,
        ExerciseType type,
        string statement,
        int displayOrder,
        string? correctAnswer = null,
        string? audioResourceUrl = null)
        : base()
    {
        if (lessonId == Guid.Empty)
            throw new ArgumentException("Lesson ID cannot be empty.", nameof(lessonId));

        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException("Statement cannot be empty.", nameof(statement));

        if (displayOrder < 1)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "Display order must be at least 1.");

        LessonId = lessonId;
        Type = type;
        Statement = statement.Trim();
        DisplayOrder = displayOrder;
        CorrectAnswer = correctAnswer?.Trim();
        AudioResourceUrl = audioResourceUrl?.Trim();
    }
}
