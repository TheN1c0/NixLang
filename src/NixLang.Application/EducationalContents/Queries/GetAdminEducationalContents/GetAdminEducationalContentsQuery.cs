using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NixLang.Application.Common.Models;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;

namespace NixLang.Application.EducationalContents.Queries.GetAdminEducationalContents;

public record AdminEducationalContentSummaryDto(
    Guid Id,
    string Title,
    string Summary,
    string Type,
    string? ReferenceLevel,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetAdminEducationalContentsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Type = null,
    string? Status = null) : IRequest<PagedResult<AdminEducationalContentSummaryDto>>;

public class GetAdminEducationalContentsQueryHandler : IRequestHandler<GetAdminEducationalContentsQuery, PagedResult<AdminEducationalContentSummaryDto>>
{
    private readonly IEducationalContentRepository _repository;

    public GetAdminEducationalContentsQueryHandler(IEducationalContentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<AdminEducationalContentSummaryDto>> Handle(GetAdminEducationalContentsQuery request, CancellationToken cancellationToken)
    {
        EducationalContentType? typeFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<EducationalContentType>(request.Type, true, out var parsedType))
        {
            typeFilter = parsedType;
        }

        PublicationStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<PublicationStatus>(request.Status, true, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        var items = await _repository.GetAllAsync(
            request.Page,
            request.PageSize,
            request.Search,
            typeFilter,
            statusFilter,
            cancellationToken);

        var totalCount = await _repository.CountAllAsync(
            request.Search,
            typeFilter,
            statusFilter,
            cancellationToken);

        var dtos = items.Select(c => new AdminEducationalContentSummaryDto(
            c.Id,
            c.Title,
            c.Summary,
            c.Type.ToString(),
            c.ReferenceLevel?.ToString(),
            c.Status.ToString(),
            c.CreatedAt,
            c.UpdatedAt)).ToList();

        return new PagedResult<AdminEducationalContentSummaryDto>(dtos, request.Page, request.PageSize, totalCount);
    }
}
