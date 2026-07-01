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
    public ExerciseType Type { get; private set; }
    public string Statement { get; private set; }
    public string? CorrectAnswer { get; private set; }
    public string? AudioResourceUrl { get; private set; }

    protected Exercise() : base()
    {
        Statement = string.Empty;
    }

    public Exercise(
        ExerciseType type,
        string statement,
        string? correctAnswer = null,
        string? audioResourceUrl = null)
        : base()
    {
        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException("Statement cannot be empty.", nameof(statement));

        Type = type;
        Statement = statement.Trim();
        CorrectAnswer = correctAnswer?.Trim();
        AudioResourceUrl = audioResourceUrl?.Trim();
    }
}
