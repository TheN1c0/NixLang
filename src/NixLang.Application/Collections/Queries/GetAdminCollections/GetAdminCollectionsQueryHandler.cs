using MediatR;
using NixLang.Application.Common.Models;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Collections.Queries.GetAdminCollections;

public class GetAdminCollectionsQueryHandler : IRequestHandler<GetAdminCollectionsQuery, PagedResult<AdminCollectionSummaryDto>>
{
    private readonly ICollectionRepository _collectionRepository;

    public GetAdminCollectionsQueryHandler(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<PagedResult<AdminCollectionSummaryDto>> Handle(GetAdminCollectionsQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _collectionRepository.CountAllAsync(cancellationToken);
        var collections = await _collectionRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);

        var items = collections.Select(c => new AdminCollectionSummaryDto(
            c.Id,
            c.Title,
            c.Description,
            c.IconUrl,
            c.SuggestedLevel?.ToString(),
            c.Status.ToString(),
            c.DisplayOrder,
            c.CollectionLessons.Count,
            c.CreatedAt,
            c.UpdatedAt)).ToList().AsReadOnly();

        return new PagedResult<AdminCollectionSummaryDto>(items, request.Page, request.PageSize, totalCount);
    }
}
