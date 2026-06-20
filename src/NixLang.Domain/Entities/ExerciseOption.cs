using NixLang.Domain.Common;

namespace NixLang.Domain.Entities;

/// <summary>
/// Answer option for multiple-choice exercises.
/// Belongs to the Lesson Aggregate.
/// Maps to: AlternativaEjercicio.
/// Source: RN-14.
/// </summary>
public class ExerciseOption : BaseEntity
{
    public Guid ExerciseId { get; private set; }
    public string Text { get; private set; }
    public bool IsCorrect { get; private set; }
    public int DisplayOrder { get; private set; }

    protected ExerciseOption() : base()
    {
        Text = string.Empty;
    }

    public ExerciseOption(Guid exerciseId, string text, bool isCorrect, int displayOrder)
        : base()
    {
        if (exerciseId == Guid.Empty)
            throw new ArgumentException("Exercise ID cannot be empty.", nameof(exerciseId));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Option text cannot be empty.", nameof(text));

        if (displayOrder < 1)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), "Display order must be at least 1.");

        ExerciseId = exerciseId;
        Text = text.Trim();
        IsCorrect = isCorrect;
        DisplayOrder = displayOrder;
    }
}
