using FluentValidation;
using NixLang.Domain.Enums;

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

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage("Search term must not exceed 100 characters.");

        RuleFor(x => x.Level)
            .Must(level => level == null || Enum.TryParse<ReferenceLevel>(level, true, out _))
            .WithMessage("Invalid reference level. Supported levels: A1, A2, B1, B2.");
    }
}
