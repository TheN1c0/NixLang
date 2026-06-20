using NixLang.Domain.Common;

namespace NixLang.Domain.Entities;

/// <summary>
/// Thematic grouping for organizing and classifying lessons.
/// Aggregate Root of the Category Aggregate.
/// Maps to: Categoría.
/// Source: RN-19, RN-20, RN-21.
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    protected Category() : base()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Category(string name, string description)
        : base()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description cannot be empty.", nameof(description));

        Name = name.Trim();
        Description = description.Trim();
    }
}
