using MediatR;
using NixLang.Application.Exercises.Commands.CreateExercise;

namespace NixLang.Application.Exercises.Commands.UpdateExercise;

public record UpdateExerciseCommand(
    Guid Id,
    string Type,
    string Statement,
    string? CorrectAnswer,
    string? AudioResourceUrl,
    List<CreateExerciseOptionDto>? Options) : IRequest<bool>;
