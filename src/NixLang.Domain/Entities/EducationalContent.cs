using System;
using NixLang.Domain.Common;
using NixLang.Domain.Enums;

namespace NixLang.Domain.Entities;

/// <summary>
/// Independent educational content unit (concept, rule, tip, explanation, example, etc.).
/// Aggregate Root of the EducationalContent Aggregate.
/// </summary>
public class EducationalContent : BaseEntity
{
    public string Title { get; private set; }
    public string Summary { get; private set; }
    public string Body { get; private set; }
    public EducationalContentType Type { get; private set; }
    public ReferenceLevel? ReferenceLevel { get; private set; }
    public PublicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected EducationalContent() : base()
    {
        Title = string.Empty;
        Summary = string.Empty;
        Body = string.Empty;
    }

    public EducationalContent(
        string title,
        string summary,
        string body,
        EducationalContentType type,
        ReferenceLevel? referenceLevel = null) : base()
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty.", nameof(body));

        Title = title.Trim();
        Summary = summary?.Trim() ?? string.Empty;
        Body = body.Trim();
        Type = type;
        ReferenceLevel = referenceLevel;
        Status = PublicationStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string title,
        string summary,
        string body,
        EducationalContentType type,
        ReferenceLevel? referenceLevel = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty.", nameof(body));

        Title = title.Trim();
        Summary = summary?.Trim() ?? string.Empty;
        Body = body.Trim();
        Type = type;
        ReferenceLevel = referenceLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        Status = PublicationStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDraft()
    {
        Status = PublicationStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = PublicationStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
    }
}
