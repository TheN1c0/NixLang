using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Domain.Repositories;

namespace NixLang.Application.EducationalContents.Queries.GetEducationalContentById;

public record EducationalContentDetailDto(
    Guid Id,
    string Title,
    string Summary,
    string Body,
    string Type,
    string? ReferenceLevel,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetEducationalContentByIdQuery(Guid Id) : IRequest<EducationalContentDetailDto>;

public class GetEducationalContentByIdQueryHandler : IRequestHandler<GetEducationalContentByIdQuery, EducationalContentDetailDto>
{
    private readonly IEducationalContentRepository _repository;

    public GetEducationalContentByIdQueryHandler(IEducationalContentRepository repository)
    {
        _repository = repository;
    }

    public async Task<EducationalContentDetailDto> Handle(GetEducationalContentByIdQuery request, CancellationToken cancellationToken)
    {
        var content = await _repository.GetPublishedByIdAsync(request.Id, cancellationToken);
        if (content == null)
        {
            throw new EducationalContentNotFoundException(request.Id);
        }

        return new EducationalContentDetailDto(
            content.Id,
            content.Title,
            content.Summary,
            content.Body,
            content.Type.ToString(),
            content.ReferenceLevel?.ToString(),
            content.CreatedAt,
            content.UpdatedAt);
    }
}
