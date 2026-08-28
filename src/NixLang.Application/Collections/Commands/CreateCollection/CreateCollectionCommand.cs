using FluentValidation;
using MediatR;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Collections.Commands.CreateCollection;

public record CreateCollectionCommand(
    string Title,
    string Description,
    string? IconUrl = null,
    string? SuggestedLevel = null,
    int DisplayOrder = 0,
    List<Guid>? LessonIds = null) : IRequest<Guid>;

public class CreateCollectionCommandValidator : AbstractValidator<CreateCollectionCommand>
{
    public CreateCollectionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title cannot exceed 300 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.IconUrl)
            .MaximumLength(500).WithMessage("IconUrl cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.IconUrl));

        RuleFor(x => x.SuggestedLevel)
            .Must(level => string.IsNullOrEmpty(level) || Enum.TryParse<ReferenceLevel>(level, true, out _))
            .WithMessage("Invalid suggested level. Allowed values: A1, A2, B1, B2.");
    }
}

public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, Guid>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IUnitOfWork unitOfWork)
    {
        _collectionRepository = collectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
    {
        ReferenceLevel? suggestedLevel = null;
        if (!string.IsNullOrWhiteSpace(request.SuggestedLevel) &&
            Enum.TryParse<ReferenceLevel>(request.SuggestedLevel, true, out var level))
        {
            suggestedLevel = level;
        }

        var collection = new Collection(
            request.Title,
            request.Description,
            request.IconUrl,
            suggestedLevel,
            request.DisplayOrder);

        if (request.LessonIds != null && request.LessonIds.Any())
        {
            foreach (var lessonId in request.LessonIds)
            {
                collection.AddLesson(lessonId);
            }
        }

        await _collectionRepository.AddAsync(collection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return collection.Id;
    }
}
