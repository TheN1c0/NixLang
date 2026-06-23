using System;

namespace NixLang.Application.Lessons.Queries.GetLessonById;

/// <summary>
/// DTO representing detailed information of a lesson, including the total number of exercises.
/// </summary>
public record LessonDetailDto(
    Guid Id,
    string Title,
    string Description,
    string ReferenceLevel,
    int ExerciseCount);
