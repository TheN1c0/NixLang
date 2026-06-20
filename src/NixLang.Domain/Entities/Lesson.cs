using NixLang.Domain.Common;
using NixLang.Domain.Enums;

namespace NixLang.Domain.Entities;

/// <summary>
/// Main pedagogical unit of the platform.
/// Aggregate Root of the Lesson Aggregate.
/// Maps to: Lección.
/// Source: RN-06, RN-07, RN-08, RN-11.
/// </summary>
public class Lesson : BaseEntity
{
    private readonly List<Exercise> _exercises = [];

    public string Title { get; private set; }
    public string Description { get; private set; }
    public ReferenceLevel ReferenceLevel { get; private set; }
    public PublicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    protected Lesson() : base()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Lesson(string title, string description, ReferenceLevel referenceLevel)
        : base()
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title.Trim();
        Description = description.Trim();
        ReferenceLevel = referenceLevel;
        Status = PublicationStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }
}
