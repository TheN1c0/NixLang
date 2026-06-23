using FluentValidation;

namespace NixLang.Application.Lessons.Queries.GetLessons;

public class GetLessonsQueryValidator : AbstractValidator<GetLessonsQuery>
{
    public GetLessonsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(50)
            .WithMessage("Page size must not exceed 50.");
    }
}
