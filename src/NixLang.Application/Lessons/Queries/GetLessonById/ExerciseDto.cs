using System.Collections.Generic;

namespace NixLang.Application.Lessons.Queries.GetLessonById;

public record ExerciseOptionDto(
    Guid Id,
    string Text,
    int DisplayOrder);

public record ExerciseDto(
    Guid Id,
    string Type,
    string Statement,
    string? CorrectAnswer,
    string? AudioResourceUrl,
    IReadOnlyList<ExerciseOptionDto> Options);
