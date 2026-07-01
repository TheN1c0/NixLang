using System;

namespace NixLang.Application.Lessons.Queries.GetLessonById;

public record LessonBlockDto(
    Guid Id,
    string Type,
    int Sequence,
    string ConfigurationValue,
    Guid? ReferencedExerciseId,
    ExerciseDto? Exercise);
