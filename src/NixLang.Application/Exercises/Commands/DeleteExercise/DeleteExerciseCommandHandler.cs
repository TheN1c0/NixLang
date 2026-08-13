using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Exercises.Commands.DeleteExercise;

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand, bool>
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExerciseCommandHandler(IExerciseRepository exerciseRepository, IUnitOfWork unitOfWork)
    {
        _exerciseRepository = exerciseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (exercise == null)
        {
            throw new ExerciseNotFoundException(request.Id);
        }

        // Integrity validation (RN-32 / RN-090)
        if (await _exerciseRepository.IsExerciseUsedInLessonAsync(request.Id, cancellationToken))
        {
            throw new ExerciseInUseException(request.Id);
        }

        await _exerciseRepository.DeleteAsync(exercise, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
