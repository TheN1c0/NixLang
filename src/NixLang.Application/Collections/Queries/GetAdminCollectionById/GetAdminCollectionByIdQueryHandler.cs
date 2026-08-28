using MediatR;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Collections.Queries.GetAdminCollectionById;

public class GetAdminCollectionByIdQueryHandler : IRequestHandler<GetAdminCollectionByIdQuery, AdminCollectionDetailDto?>
{
    private readonly ICollectionRepository _collectionRepository;

    public GetAdminCollectionByIdQueryHandler(ICollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<AdminCollectionDetailDto?> Handle(GetAdminCollectionByIdQuery request, CancellationToken cancellationToken)
    {
        var collection = await _collectionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (collection == null)
            return null;

        var lessons = collection.CollectionLessons
            .OrderBy(cl => cl.Order)
            .Select(cl => new AdminCollectionLessonItemDto(
                cl.Id,
                cl.LessonId,
                cl.Lesson?.Title ?? "Unknown Lesson",
                cl.Lesson?.ReferenceLevel.ToString() ?? "A1",
                cl.Lesson?.Status.ToString() ?? "Draft",
                cl.Order))
            .ToList()
            .AsReadOnly();

        return new AdminCollectionDetailDto(
            collection.Id,
            collection.Title,
            collection.Description,
            collection.IconUrl,
            collection.SuggestedLevel?.ToString(),
            collection.Status.ToString(),
            collection.DisplayOrder,
            collection.CreatedAt,
            collection.UpdatedAt,
            lessons);
    }
}
