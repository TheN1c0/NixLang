using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.EducationalContents.Commands.CreateEducationalContent;

public record CreateEducationalContentCommand(
    string Title,
    string Summary,
    string Body,
    string Type,
    string? ReferenceLevel = null,
    string? Status = null) : IRequest<Guid>;

public class CreateEducationalContentCommandHandler : IRequestHandler<CreateEducationalContentCommand, Guid>
{
    private readonly IEducationalContentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEducationalContentCommandHandler(
        IEducationalContentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateEducationalContentCommand request, CancellationToken cancellationToken)
    {
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

        var content = new EducationalContent(
            request.Title,
            request.Summary,
            request.Body,
            type,
            referenceLevel);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<PublicationStatus>(request.Status, true, out var status))
            {
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
            }
        }

        await _repository.AddAsync(content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return content.Id;
    }
}
