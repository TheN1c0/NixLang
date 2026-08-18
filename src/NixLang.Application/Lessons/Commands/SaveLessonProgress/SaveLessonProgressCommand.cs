using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<SaveLessonProgressCommandHandler> _logger;

    public SaveLessonProgressCommandHandler(
        ILessonProgressRepository progressRepository,
        ILessonRepository lessonRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<SaveLessonProgressCommandHandler> logger)
    {
        _progressRepository = progressRepository;
        _lessonRepository = lessonRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SaveLessonProgressCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        _logger.LogWarning("[DIAG-SaveProgress] RECEIVED: UserId={UserId}, LessonId={LessonId}, Percentage={Pct}%, Status='{Status}', ResultsCount={ResultsCount}",
            userId, request.LessonId, request.ProgressPercentage, request.Status, request.Results?.Count ?? 0);

        // 1. Verify lesson exists
        var lesson = await _lessonRepository.GetPublishedByIdAsync(request.LessonId, cancellationToken);

        if (lesson == null)
        {
            throw new LessonNotFoundException(request.LessonId);
        }

        _logger.LogWarning("[DIAG-SaveProgress] Lesson found: Title='{Title}', BlockCount={BlockCount}",
            lesson.Title, lesson.LessonBlocks.Count);

        // 2. Parse status enum
        if (!Enum.TryParse<ProgressStatus>(request.Status, true, out var status))
        {
            _logger.LogWarning("[DIAG-SaveProgress] ⚠️ FAILED to parse status '{Status}', defaulting to InProgress", request.Status);
            status = ProgressStatus.InProgress;
        }
        else
        {
            _logger.LogWarning("[DIAG-SaveProgress] Parsed status: '{Status}' -> {ParsedStatus}", request.Status, status);
        }

        // 3. Find or Create Progress record
        var progress = await _progressRepository.GetAsync(userId, request.LessonId, cancellationToken);

        if (progress == null)
        {
            _logger.LogWarning("[DIAG-SaveProgress] No existing progress record, CREATING NEW one");
            progress = new LessonProgress(userId, request.LessonId);
            await _progressRepository.AddAsync(progress, cancellationToken);
        }
        else
        {
            _logger.LogWarning("[DIAG-SaveProgress] Existing progress: Id={Id}, CurrentPct={Pct}%, CurrentStatus={Status}",
                progress.Id, progress.ProgressPercentage, progress.Status);
        }

        // 4. Update core properties
        progress.UpdateProgress(request.ProgressPercentage, status);

        _logger.LogWarning("[DIAG-SaveProgress] AFTER UPDATE: Pct={Pct}%, Status={Status}, CompletedAt={CompletedAt}",
            progress.ProgressPercentage, progress.Status, progress.CompletedAt);

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

        _logger.LogWarning("[DIAG-SaveProgress] ✅ SAVED SUCCESSFULLY: LessonId={LessonId}, FinalPct={Pct}%, FinalStatus={Status}",
            request.LessonId, progress.ProgressPercentage, progress.Status);

        return true;
    }
}

