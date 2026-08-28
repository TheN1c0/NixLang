using NixLang.Domain.Common;

namespace NixLang.Domain.Entities;

/// <summary>
/// Ordered association between a Collection and a Lesson.
/// Preserves the suggested pedagogical sequence of the lesson within the collection.
/// Maps to: ColecciónLección.
/// Source: RN-37, RN-38.
/// </summary>
public class CollectionLesson : BaseEntity
{
    public Guid CollectionId { get; private set; }
    public Guid LessonId { get; private set; }
    public int Order { get; internal set; }

    public Lesson? Lesson { get; private set; }

    protected CollectionLesson() : base()
    {
    }

    public CollectionLesson(Guid collectionId, Guid lessonId, int order) : base()
    {
        if (collectionId == Guid.Empty)
            throw new ArgumentException("CollectionId cannot be empty.", nameof(collectionId));

        if (lessonId == Guid.Empty)
            throw new ArgumentException("LessonId cannot be empty.", nameof(lessonId));

        if (order < 1)
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be greater than or equal to 1.");

        CollectionId = collectionId;
        LessonId = lessonId;
        Order = order;
    }
}
