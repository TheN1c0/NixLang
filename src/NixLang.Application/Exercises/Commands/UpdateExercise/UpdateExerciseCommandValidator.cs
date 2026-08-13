using FluentValidation;
using NixLang.Domain.Enums;

namespace NixLang.Application.Exercises.Commands.UpdateExercise;

public class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
{
    public UpdateExerciseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Exercise ID is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Exercise type is required.")
            .Must(type => Enum.TryParse<ExerciseType>(type, true, out _))
            .WithMessage("Invalid exercise type.");

        RuleFor(x => x.Statement)
            .NotEmpty().WithMessage("Statement is required.")
            .MaximumLength(500).WithMessage("Statement must not exceed 500 characters.");

        RuleFor(x => x.CorrectAnswer)
            .NotEmpty()
            .When(x => IsAnswerRequired(x.Type))
            .WithMessage("Correct answer is required for this exercise type.");

        RuleFor(x => x.Options)
            .Must(options => options != null && options.Count >= 2)
            .When(x => IsMultipleChoice(x.Type))
            .WithMessage("Multiple choice exercises must have at least 2 options.");

        RuleFor(x => x.Options)
            .Must(options => options != null && options.Any(o => o.IsCorrect))
            .When(x => IsMultipleChoice(x.Type))
            .WithMessage("Multiple choice exercises must have at least one correct option.");
    }

    private bool IsAnswerRequired(string typeStr)
    {
        if (Enum.TryParse<ExerciseType>(typeStr, true, out var type))
        {
            return type == ExerciseType.Translation || type == ExerciseType.FillInTheBlank;
        }
        return false;
    }

    private bool IsMultipleChoice(string typeStr)
    {
        if (Enum.TryParse<ExerciseType>(typeStr, true, out var type))
        {
            return type == ExerciseType.MultipleChoice;
        }
        return false;
    }
}
