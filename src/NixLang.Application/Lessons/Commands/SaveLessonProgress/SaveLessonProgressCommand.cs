using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Application.Lessons.Commands.SaveLessonProgress;

public record SaveExerciseResultDto(Guid ExerciseId, string GivenAnswer, bool IsCorrect);

public record SaveLessonProgressCommand(
    Guid LessonId,
    decimal ProgressPercentage,
    string Status,
    List<SaveExerciseResultDto> Results) : IRequest<bool>;

public class SaveLessonProgressCommandHandler : IRequestHandler<SaveLessonProgressCommand, bool>
{
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SaveLessonProgressCommandHandler(
        ILessonProgressRepository progressRepository,
        ILessonRepository lessonRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _progressRepository = progressRepository;
        _lessonRepository = lessonRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SaveLessonProgressCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Verify lesson exists
        var lesson = await _lessonRepository.GetPublishedByIdAsync(request.LessonId, cancellationToken);

        if (lesson == null)
        {
            throw new LessonNotFoundException(request.LessonId);
        }

        // 2. Parse status enum
        if (!Enum.TryParse<ProgressStatus>(request.Status, true, out var status))
        {
            status = ProgressStatus.InProgress;
        }

        // 3. Find or Create Progress record
        var progress = await _progressRepository.GetAsync(userId, request.LessonId, cancellationToken);

        if (progress == null)
        {
            progress = new LessonProgress(userId, request.LessonId);
            await _progressRepository.AddAsync(progress, cancellationToken);
        }

        // 4. Update core properties
        progress.UpdateProgress(request.ProgressPercentage, status);

        // 5. Update exercise results
        if (request.Results != null)
        {
            foreach (var result in request.Results)
            {
                progress.AddExerciseResult(result.ExerciseId, result.GivenAnswer, result.IsCorrect);
            }
        }

        // 6. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
