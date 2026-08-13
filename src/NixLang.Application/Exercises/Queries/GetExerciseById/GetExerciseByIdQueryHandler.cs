using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Exercises.Queries.GetExercises;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Exercises.Queries.GetExerciseById;

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDto>
{
    private readonly IExerciseRepository _exerciseRepository;

    public GetExerciseByIdQueryHandler(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<ExerciseDto> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _exerciseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (e == null)
        {
            throw new ExerciseNotFoundException(request.Id);
        }

        return new ExerciseDto(
            e.Id,
            e.Type.ToString(),
            e.Statement,
            e.CorrectAnswer,
            e.AudioResourceUrl,
            e.Options.Select(o => new ExerciseOptionDto(o.Id, o.Text, o.IsCorrect, o.DisplayOrder)).ToList()
        );
    }
}
