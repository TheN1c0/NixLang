using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Exercises.Commands.UpdateExercise;

public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand, bool>
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateExerciseCommandHandler(IExerciseRepository exerciseRepository, IUnitOfWork unitOfWork)
    {
        _exerciseRepository = exerciseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (exercise == null)
        {
            throw new ExerciseNotFoundException(request.Id);
        }

        if (!Enum.TryParse<ExerciseType>(request.Type, true, out var type))
        {
            throw new ArgumentException($"Invalid exercise type: {request.Type}", nameof(request.Type));
        }

        exercise.Update(type, request.Statement, request.CorrectAnswer, request.AudioResourceUrl);

        await _exerciseRepository.ClearOptionsAsync(exercise.Id, cancellationToken);
        exercise.ClearOptions();

        if (type == ExerciseType.MultipleChoice && request.Options != null)
        {
            foreach (var opt in request.Options)
            {
                var option = new ExerciseOption(exercise.Id, opt.Text, opt.IsCorrect, opt.DisplayOrder);
                await _exerciseRepository.AddOptionAsync(option, cancellationToken);
                exercise.AddOption(option);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
