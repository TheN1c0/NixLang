namespace NixLang.Application.Lessons.Queries.GetLessons;

/// <summary>
/// DTO representing a lesson in the catalog listing.
/// Contains only summary-level data, no exercises or detailed content.
/// </summary>
public record LessonSummaryDto(Guid Id, string Title, string Description, string ReferenceLevel);
