using MediatR;

namespace NixLang.Application.Exercises.Commands.CreateExercise;

public record CreateExerciseOptionDto(string Text, bool IsCorrect, int DisplayOrder);

public record CreateExerciseCommand(
    string Type,
    string Statement,
    string? CorrectAnswer,
    string? AudioResourceUrl,
    List<CreateExerciseOptionDto>? Options) : IRequest<Guid>;
