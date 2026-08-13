using MediatR;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Exercises.Commands.CreateExercise;

public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, Guid>
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateExerciseCommandHandler(IExerciseRepository exerciseRepository, IUnitOfWork unitOfWork)
    {
        _exerciseRepository = exerciseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ExerciseType>(request.Type, true, out var type))
        {
            throw new ArgumentException($"Invalid exercise type: {request.Type}", nameof(request.Type));
        }

        var exercise = new Exercise(
            type,
            request.Statement,
            request.CorrectAnswer,
            request.AudioResourceUrl);

        if (type == ExerciseType.MultipleChoice && request.Options != null)
        {
            foreach (var opt in request.Options)
            {
                exercise.AddOption(opt.Text, opt.IsCorrect, opt.DisplayOrder);
            }
        }

        await _exerciseRepository.AddAsync(exercise, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return exercise.Id;
    }
}
