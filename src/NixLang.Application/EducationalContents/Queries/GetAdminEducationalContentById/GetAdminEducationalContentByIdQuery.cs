using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Domain.Repositories;

namespace NixLang.Application.EducationalContents.Queries.GetAdminEducationalContentById;

public record AdminEducationalContentDetailDto(
    Guid Id,
    string Title,
    string Summary,
    string Body,
    string Type,
    string? ReferenceLevel,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetAdminEducationalContentByIdQuery(Guid Id) : IRequest<AdminEducationalContentDetailDto>;

public class GetAdminEducationalContentByIdQueryHandler : IRequestHandler<GetAdminEducationalContentByIdQuery, AdminEducationalContentDetailDto>
{
    private readonly IEducationalContentRepository _repository;

    public GetAdminEducationalContentByIdQueryHandler(IEducationalContentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminEducationalContentDetailDto> Handle(GetAdminEducationalContentByIdQuery request, CancellationToken cancellationToken)
    {
        var content = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (content == null)
        {
            throw new EducationalContentNotFoundException(request.Id);
        }

        return new AdminEducationalContentDetailDto(
            content.Id,
            content.Title,
            content.Summary,
            content.Body,
            content.Type.ToString(),
            content.ReferenceLevel?.ToString(),
            content.Status.ToString(),
            content.CreatedAt,
            content.UpdatedAt);
    }
}
