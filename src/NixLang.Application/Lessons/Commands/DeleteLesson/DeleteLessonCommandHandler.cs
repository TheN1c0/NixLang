using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Lessons.Commands.DeleteLesson;

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, bool>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLessonCommandHandler(ILessonRepository lessonRepository, IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lesson == null)
        {
            throw new LessonNotFoundException(request.Id);
        }

        await _lessonRepository.DeleteAsync(lesson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
