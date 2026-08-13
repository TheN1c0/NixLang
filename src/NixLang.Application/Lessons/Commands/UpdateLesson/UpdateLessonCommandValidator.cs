using FluentValidation;
using NixLang.Domain.Enums;

namespace NixLang.Application.Lessons.Commands.UpdateLesson;

public class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lesson ID is required.");

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

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => Enum.TryParse<PublicationStatus>(status, true, out _))
            .WithMessage("Invalid publication status.");

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
