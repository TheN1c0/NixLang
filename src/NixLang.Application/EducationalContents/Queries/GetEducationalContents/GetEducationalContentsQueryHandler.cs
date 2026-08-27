using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Models;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.EducationalContents.Queries.GetEducationalContents;

public class GetEducationalContentsQueryHandler : IRequestHandler<GetEducationalContentsQuery, PagedResult<EducationalContentItemDto>>
{
    private readonly IEducationalContentRepository _repository;

    public GetEducationalContentsQueryHandler(IEducationalContentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EducationalContentItemDto>> Handle(GetEducationalContentsQuery request, CancellationToken cancellationToken)
    {
        EducationalContentType? typeFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<EducationalContentType>(request.Type, true, out var parsedType))
        {
            typeFilter = parsedType;
        }

        ReferenceLevel? levelFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Level) && Enum.TryParse<ReferenceLevel>(request.Level, true, out var parsedLevel))
        {
            levelFilter = parsedLevel;
        }

        var items = await _repository.GetPublishedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            typeFilter,
            levelFilter,
            cancellationToken);

        var totalCount = await _repository.CountPublishedAsync(
            request.Search,
            typeFilter,
            levelFilter,
            cancellationToken);

        var dtos = items.Select(c => new EducationalContentItemDto(
            c.Id,
            c.Title,
            c.Summary,
            c.Body,
            c.Type.ToString(),
            c.ReferenceLevel?.ToString(),
            c.CreatedAt)).ToList();

        return new PagedResult<EducationalContentItemDto>(dtos, request.Page, request.PageSize, totalCount);
    }
}
