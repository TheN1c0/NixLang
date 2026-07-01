using System;

namespace NixLang.Application.Lessons.Queries.GetLessonById;

public record ExerciseDto(
    Guid Id,
    string Type,
    string Statement,
    string? CorrectAnswer,
    string? AudioResourceUrl);
