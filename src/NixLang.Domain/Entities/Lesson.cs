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
    private readonly List<Category> _categories = [];

    public string Title { get; private set; }
    public string Description { get; private set; }
    public ReferenceLevel ReferenceLevel { get; private set; }
    public PublicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public void AddCategory(Category category)
    {
        if (category == null) throw new ArgumentNullException(nameof(category));
        if (!_categories.Any(c => c.Id == category.Id))
        {
            _categories.Add(category);
        }
    }

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

    /// <summary>
    /// Transitions the lesson to Published status, making it visible in the catalog.
    /// </summary>
    public void Publish()
    {
        Status = PublicationStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transitions the lesson to Disabled status, removing it from the catalog.
    /// </summary>
    public void Disable()
    {
        Status = PublicationStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
    }
}
