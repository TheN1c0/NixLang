using MediatR;
using NixLang.Application.Common.Models;

namespace NixLang.Application.Exercises.Queries.GetExercises;

public record ExerciseOptionDto(Guid Id, string Text, bool IsCorrect, int DisplayOrder);

public record ExerciseDto(
    Guid Id,
    string Type,
    string Statement,
    string? CorrectAnswer,
    string? AudioResourceUrl,
    List<ExerciseOptionDto> Options);

public record GetExercisesQuery(int Page = 1, int PageSize = 10, string? Search = null) : IRequest<PagedResult<ExerciseDto>>;
