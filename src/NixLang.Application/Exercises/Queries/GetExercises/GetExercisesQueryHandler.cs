using MediatR;
using NixLang.Application.Common.Models;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Exercises.Queries.GetExercises;

public class GetExercisesQueryHandler : IRequestHandler<GetExercisesQuery, PagedResult<ExerciseDto>>
{
    private readonly IExerciseRepository _exerciseRepository;

    public GetExercisesQueryHandler(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<PagedResult<ExerciseDto>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
    {
        var exercises = await _exerciseRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);

        var totalCount = await _exerciseRepository.CountAllAsync(request.Search, cancellationToken);

        var dtos = exercises.Select(e => new ExerciseDto(
            e.Id,
            e.Type.ToString(),
            e.Statement,
            e.CorrectAnswer,
            e.AudioResourceUrl,
            e.Options.Select(o => new ExerciseOptionDto(o.Id, o.Text, o.IsCorrect, o.DisplayOrder)).ToList()
        )).ToList();

        return new PagedResult<ExerciseDto>(dtos, request.Page, request.PageSize, totalCount);
    }
}
