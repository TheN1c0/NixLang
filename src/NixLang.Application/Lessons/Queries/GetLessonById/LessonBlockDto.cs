using System;

namespace NixLang.Application.Lessons.Queries.GetLessonById;

public record EducationalContentSummaryDto(
    Guid Id,
    string Title,
    string Summary,
    string Body,
    string Type,
    string? ReferenceLevel);

public record LessonBlockDto(
    Guid Id,
    string Type,
    int Sequence,
    string ConfigurationValue,
    Guid? ReferencedExerciseId,
    ExerciseDto? Exercise,
    Guid? ReferencedEducationalContentId = null,
    EducationalContentSummaryDto? EducationalContent = null);
