using MediatR;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.Application.Lessons.Commands.CreateLesson;

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, Guid>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLessonCommandHandler(
        ILessonRepository lessonRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _lessonRepository = lessonRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ReferenceLevel>(request.ReferenceLevel, true, out var level))
        {
            throw new ArgumentException($"Invalid reference level: {request.ReferenceLevel}", nameof(request.ReferenceLevel));
        }

        var lesson = new Lesson(request.Title, request.Description, level);

        // Add categories if specified
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

        // Add blocks if specified
        if (request.LessonBlocks != null && request.LessonBlocks.Count > 0)
        {
            var hasExercise = request.LessonBlocks.Any(b => 
                string.Equals(b.Type, LessonBlockType.Exercise.ToString(), StringComparison.OrdinalIgnoreCase));

            if (!hasExercise)
            {
                throw new ArgumentException("A lesson must contain at least one exercise.");
            }

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

                lesson.AddLessonBlock(block);
            }
        }
        else
        {
            throw new ArgumentException("A lesson must contain at least one exercise.");
        }

        await _lessonRepository.AddAsync(lesson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lesson.Id;
    }
}
