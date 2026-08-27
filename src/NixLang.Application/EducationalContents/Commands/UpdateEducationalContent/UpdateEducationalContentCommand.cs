using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.EducationalContents.Commands.UpdateEducationalContent;

public record UpdateEducationalContentCommand(
    Guid Id,
    string Title,
    string Summary,
    string Body,
    string Type,
    string? ReferenceLevel,
    string Status) : IRequest<bool>;

public class UpdateEducationalContentCommandHandler : IRequestHandler<UpdateEducationalContentCommand, bool>
{
    private readonly IEducationalContentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEducationalContentCommandHandler(
        IEducationalContentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateEducationalContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (content == null)
        {
            throw new EducationalContentNotFoundException(request.Id);
        }

        if (!Enum.TryParse<EducationalContentType>(request.Type, true, out var type))
        {
            throw new ArgumentException($"Invalid educational content type: {request.Type}", nameof(request.Type));
        }

        ReferenceLevel? referenceLevel = null;
        if (!string.IsNullOrWhiteSpace(request.ReferenceLevel))
        {
            if (Enum.TryParse<ReferenceLevel>(request.ReferenceLevel, true, out var parsedLevel))
            {
                referenceLevel = parsedLevel;
            }
            else
            {
                throw new ArgumentException($"Invalid reference level: {request.ReferenceLevel}", nameof(request.ReferenceLevel));
            }
        }

        if (!Enum.TryParse<PublicationStatus>(request.Status, true, out var status))
        {
            throw new ArgumentException($"Invalid status: {request.Status}", nameof(request.Status));
        }

        content.Update(request.Title, request.Summary, request.Body, type, referenceLevel);

        switch (status)
        {
            case PublicationStatus.Published:
                content.Publish();
                break;
            case PublicationStatus.Disabled:
                content.Disable();
                break;
            case PublicationStatus.Draft:
                content.SetDraft();
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
