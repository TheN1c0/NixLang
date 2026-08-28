using FluentValidation;
using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Collections.Commands.UpdateCollection;

public record UpdateCollectionCommand(
    Guid Id,
    string Title,
    string Description,
    string? IconUrl,
    string? SuggestedLevel,
    string Status,
    int DisplayOrder,
    List<Guid>? LessonIds = null) : IRequest<bool>;

public class UpdateCollectionCommandValidator : AbstractValidator<UpdateCollectionCommand>
{
    public UpdateCollectionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

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

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<PublicationStatus>(status, true, out _))
            .WithMessage("Invalid publication status. Allowed values: Draft, Published, Disabled.");
    }
}

public class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, bool>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork)
    {
        _collectionRepository = collectionRepository;
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
    {
        var collection = await _collectionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (collection == null)
            throw new CollectionNotFoundException(request.Id);

        ReferenceLevel? suggestedLevel = null;
        if (!string.IsNullOrWhiteSpace(request.SuggestedLevel) &&
            Enum.TryParse<ReferenceLevel>(request.SuggestedLevel, true, out var level))
        {
            suggestedLevel = level;
        }

        collection.Update(
            request.Title,
            request.Description,
            request.IconUrl,
            suggestedLevel,
            request.DisplayOrder);

        // Synchronize and reorder associated lessons if provided
        if (request.LessonIds != null)
        {
            collection.SetLessons(request.LessonIds);
        }

        // Status transition handling
        if (Enum.TryParse<PublicationStatus>(request.Status, true, out var targetStatus))
        {
            if (targetStatus == PublicationStatus.Published)
            {
                // Validate RN-40: Cannot publish without at least one published lesson
                var publishedLessonsCount = collection.CollectionLessons
                    .Count(cl => cl.Lesson != null && cl.Lesson.Status == PublicationStatus.Published);

                // If navigation wasn't fully loaded, check via repository
                if (publishedLessonsCount == 0 && collection.CollectionLessons.Any())
                {
                    foreach (var cl in collection.CollectionLessons)
                    {
                        var lesson = await _lessonRepository.GetByIdAsync(cl.LessonId, cancellationToken);
                        if (lesson != null && lesson.Status == PublicationStatus.Published)
                        {
                            publishedLessonsCount++;
                            break;
                        }
                    }
                }

                if (publishedLessonsCount == 0)
                {
                    throw new InvalidOperationException("Cannot publish a collection without at least one published lesson.");
                }

                collection.Publish();
            }
            else if (targetStatus == PublicationStatus.Disabled)
            {
                collection.Disable();
            }
            else
            {
                collection.SetDraft();
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
