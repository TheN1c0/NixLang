using NixLang.Domain.Common;
using NixLang.Domain.Enums;

namespace NixLang.Domain.Entities;

/// <summary>
/// Aggregate Root representing an intentional learning experience, purpose or situational theme.
/// Groups lessons under a suggested pedagogical order without restricting direct access.
/// Maps to: Colección.
/// Source: RN-35, RN-36, RN-37, RN-38, RN-40.
/// </summary>
public class Collection : BaseEntity
{
    private readonly List<CollectionLesson> _collectionLessons = [];

    public string Title { get; private set; }
    public string Description { get; private set; }
    public string? IconUrl { get; private set; }
    public ReferenceLevel? SuggestedLevel { get; private set; }
    public PublicationStatus Status { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<CollectionLesson> CollectionLessons => 
        _collectionLessons.OrderBy(cl => cl.Order).ToList().AsReadOnly();

    protected Collection() : base()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Collection(
        string title, 
        string description, 
        string? iconUrl = null, 
        ReferenceLevel? suggestedLevel = null, 
        int displayOrder = 0) : base()
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title.Trim();
        Description = description.Trim();
        IconUrl = string.IsNullOrWhiteSpace(iconUrl) ? null : iconUrl.Trim();
        SuggestedLevel = suggestedLevel;
        Status = PublicationStatus.Draft;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string title, 
        string description, 
        string? iconUrl = null, 
        ReferenceLevel? suggestedLevel = null, 
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title.Trim();
        Description = description.Trim();
        IconUrl = string.IsNullOrWhiteSpace(iconUrl) ? null : iconUrl.Trim();
        SuggestedLevel = suggestedLevel;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDraft()
    {
        Status = PublicationStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        Status = PublicationStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = PublicationStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLesson(Guid lessonId)
    {
        if (lessonId == Guid.Empty)
            throw new ArgumentException("LessonId cannot be empty.", nameof(lessonId));

        if (_collectionLessons.Any(cl => cl.LessonId == lessonId))
            return; // Already in collection

        int nextOrder = _collectionLessons.Count > 0 ? _collectionLessons.Max(cl => cl.Order) + 1 : 1;
        var collectionLesson = new CollectionLesson(Id, lessonId, nextOrder);
        _collectionLessons.Add(collectionLesson);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveLesson(Guid lessonId)
    {
        var existing = _collectionLessons.FirstOrDefault(cl => cl.LessonId == lessonId);
        if (existing != null)
        {
            _collectionLessons.Remove(existing);
            ReorderSequences();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ReorderLessons(IEnumerable<Guid> orderedLessonIds)
    {
        if (orderedLessonIds == null)
            throw new ArgumentNullException(nameof(orderedLessonIds));

        var idList = orderedLessonIds.ToList();
        var currentIds = _collectionLessons.Select(cl => cl.LessonId).ToHashSet();

        // Check if all ordered ids exist in collection
        if (!idList.All(id => currentIds.Contains(id)) || idList.Count != _collectionLessons.Count)
        {
            throw new ArgumentException("Provided list of lesson IDs does not match existing collection lessons.", nameof(orderedLessonIds));
        }

        for (int i = 0; i < idList.Count; i++)
        {
            var item = _collectionLessons.First(cl => cl.LessonId == idList[i]);
            item.Order = i + 1;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLessons(IEnumerable<Guid> lessonIds)
    {
        var targetIds = lessonIds?.ToList() ?? new List<Guid>();

        // Remove lessons no longer present
        var toRemove = _collectionLessons.Where(cl => !targetIds.Contains(cl.LessonId)).ToList();
        foreach (var item in toRemove)
        {
            _collectionLessons.Remove(item);
        }

        // Add new or update order of existing
        for (int i = 0; i < targetIds.Count; i++)
        {
            var targetId = targetIds[i];
            var existing = _collectionLessons.FirstOrDefault(cl => cl.LessonId == targetId);
            if (existing != null)
            {
                existing.Order = i + 1;
            }
            else
            {
                var newItem = new CollectionLesson(Id, targetId, i + 1);
                _collectionLessons.Add(newItem);
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearLessons()
    {
        _collectionLessons.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    private void ReorderSequences()
    {
        var ordered = _collectionLessons.OrderBy(cl => cl.Order).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].Order = i + 1;
        }
    }
}
