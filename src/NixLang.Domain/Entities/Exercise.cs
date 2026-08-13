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

    private readonly List<ExerciseOption> _options = [];
    public IReadOnlyCollection<ExerciseOption> Options => _options.AsReadOnly();

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

    public void Update(
        ExerciseType type,
        string statement,
        string? correctAnswer = null,
        string? audioResourceUrl = null)
    {
        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException("Statement cannot be empty.", nameof(statement));

        Type = type;
        Statement = statement.Trim();
        CorrectAnswer = correctAnswer?.Trim();
        AudioResourceUrl = audioResourceUrl?.Trim();
    }

    public void AddOption(string text, bool isCorrect, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Option text cannot be empty.", nameof(text));

        _options.Add(new ExerciseOption(Id, text, isCorrect, displayOrder));
    }

    public void AddOption(ExerciseOption option)
    {
        if (option == null) throw new ArgumentNullException(nameof(option));
        if (option.ExerciseId != Id) throw new ArgumentException("Option does not belong to this exercise.", nameof(option));
        _options.Add(option);
    }

    public void ClearOptions()
    {
        _options.Clear();
    }
}
