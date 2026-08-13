using FluentValidation;
using NixLang.Domain.Enums;

namespace NixLang.Application.Lessons.Commands.CreateLesson;

public class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(150).WithMessage("Title must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.ReferenceLevel)
            .NotEmpty().WithMessage("Reference level is required.")
            .Must(level => Enum.TryParse<ReferenceLevel>(level, true, out _))
            .WithMessage("Invalid reference level.");

        RuleForEach(x => x.LessonBlocks).ChildRules(block =>
        {
            block.RuleFor(b => b.Type)
                .NotEmpty().WithMessage("Block type is required.")
                .Must(type => Enum.TryParse<LessonBlockType>(type, true, out _))
                .WithMessage("Invalid block type.");

            block.RuleFor(b => b.ReferencedExerciseId)
                .NotEmpty().When(b => string.Equals(b.Type, LessonBlockType.Exercise.ToString(), StringComparison.OrdinalIgnoreCase))
                .WithMessage("Referenced exercise ID is required for exercise blocks.");
        });
    }
}
