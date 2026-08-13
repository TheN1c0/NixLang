using MediatR;

namespace NixLang.Application.Exercises.Commands.DeleteExercise;

public record DeleteExerciseCommand(Guid Id) : IRequest<bool>;
