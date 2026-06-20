using NixLang.Domain.Common;

namespace NixLang.Domain.Entities;

/// <summary>
/// Represents a user marking a lesson as favorite.
/// Associative entity between User and Lesson aggregates.
/// Maps to: Favorito.
/// Source: RN-29, RN-30, RN-31, RN-32.
/// </summary>
public class Favorite : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public DateTime MarkedAt { get; private set; }

    protected Favorite() : base() { }

    public Favorite(Guid userId, Guid lessonId)
        : base()
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (lessonId == Guid.Empty)
            throw new ArgumentException("Lesson ID cannot be empty.", nameof(lessonId));

        UserId = userId;
        LessonId = lessonId;
        MarkedAt = DateTime.UtcNow;
    }
}
