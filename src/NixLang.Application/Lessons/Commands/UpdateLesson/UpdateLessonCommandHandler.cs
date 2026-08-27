using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Lessons.Commands.CreateLesson;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.Application.Lessons.Commands.UpdateLesson;

public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, bool>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLessonCommandHandler(
        ILessonRepository lessonRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lesson == null)
        {
            throw new LessonNotFoundException(request.Id);
        }

        if (!Enum.TryParse<ReferenceLevel>(request.ReferenceLevel, true, out var level))
        {
            throw new ArgumentException($"Invalid reference level: {request.ReferenceLevel}", nameof(request.ReferenceLevel));
        }

        if (!Enum.TryParse<PublicationStatus>(request.Status, true, out var status))
        {
            throw new ArgumentException($"Invalid status: {request.Status}", nameof(request.Status));
        }

        // Update core info
        lesson.Update(request.Title, request.Description, level);

        // Update publication status
        switch (status)
        {
            case PublicationStatus.Draft:
                lesson.SetDraft();
                break;
            case PublicationStatus.Published:
                lesson.Publish();
                break;
            case PublicationStatus.Disabled:
                lesson.Disable();
                break;
        }

        // Rebuild Categories
        lesson.ClearCategories();
        if (request.CategoryIds != null && request.CategoryIds.Count > 0)
        {
            foreach (var categoryId in request.CategoryIds)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
                if (category != null)
                {
                    lesson.AddCategory(category);
                }
            }
        }

        // Rebuild Blocks
        if (request.LessonBlocks == null || !request.LessonBlocks.Any(b => 
            string.Equals(b.Type, LessonBlockType.Exercise.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("A lesson must contain at least one exercise.");
        }

        await _lessonRepository.ClearLessonBlocksAsync(lesson.Id, cancellationToken);
        lesson.ClearLessonBlocks();
        int seq = 1;
        foreach (var blockDto in request.LessonBlocks)
        {
            if (!Enum.TryParse<LessonBlockType>(blockDto.Type, true, out var blockType))
            {
                throw new ArgumentException($"Invalid lesson block type: {blockDto.Type}");
            }

            LessonBlock block;
            if (blockType == LessonBlockType.Exercise)
            {
                if (blockDto.ReferencedExerciseId == null || blockDto.ReferencedExerciseId == Guid.Empty)
                {
                    throw new ArgumentException("Exercise blocks must have a referenced exercise ID.");
                }
                block = LessonBlock.CreateExerciseBlock(lesson.Id, seq++, blockDto.ReferencedExerciseId.Value);
            }
            else if (blockType == LessonBlockType.Content)
            {
                if (blockDto.ReferencedEducationalContentId == null || blockDto.ReferencedEducationalContentId == Guid.Empty)
                {
                    throw new ArgumentException("Content blocks must have a referenced educational content ID.");
                }
                block = LessonBlock.CreateContentBlock(lesson.Id, seq++, blockDto.ReferencedEducationalContentId.Value);
            }
            else
            {
                block = LessonBlock.CreateInformationalBlock(
                    lesson.Id,
                    blockType,
                    seq++,
                    new BlockConfiguration(blockDto.ConfigurationValue));
            }

            await _lessonRepository.AddLessonBlockAsync(block, cancellationToken);
            lesson.AddLessonBlock(block);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
