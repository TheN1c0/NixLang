using MediatR;
using NixLang.Application.Exercises.Queries.GetExercises;

namespace NixLang.Application.Exercises.Queries.GetExerciseById;

public record GetExerciseByIdQuery(Guid Id) : IRequest<ExerciseDto>;
