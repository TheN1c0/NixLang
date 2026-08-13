using NixLang.Domain.Common;
using NixLang.Domain.Enums;

namespace NixLang.Domain.Entities;

/// <summary>
/// Main pedagogical unit of the platform.
/// Aggregate Root of the Lesson Aggregate.
/// Maps to: Lección.
/// Source: RN-06, RN-07, RN-08, RN-11.
/// </summary>
public class Lesson : BaseEntity
{
    private readonly List<LessonBlock> _lessonBlocks = [];
    private readonly List<Category> _categories = [];

    public string Title { get; private set; }
    public string Description { get; private set; }
    public ReferenceLevel ReferenceLevel { get; private set; }
    public PublicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<LessonBlock> LessonBlocks => _lessonBlocks.OrderBy(b => b.Sequence).ToList().AsReadOnly();
    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public void AddCategory(Category category)
    {
        if (category == null) throw new ArgumentNullException(nameof(category));
        if (!_categories.Any(c => c.Id == category.Id))
        {
            _categories.Add(category);
        }
    }

    public void AddLessonBlock(LessonBlock block)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));
        if (block.LessonId != Id) throw new ArgumentException("Block does not belong to this lesson.", nameof(block));

        int nextSequence = _lessonBlocks.Count > 0 ? _lessonBlocks.Max(b => b.Sequence) + 1 : 1;
        block.Sequence = nextSequence;
        _lessonBlocks.Add(block);
        UpdatedAt = DateTime.UtcNow;
    }

    public void InsertLessonBlock(int index, LessonBlock block)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));
        if (block.LessonId != Id) throw new ArgumentException("Block does not belong to this lesson.", nameof(block));
        if (index < 0 || index > _lessonBlocks.Count) throw new ArgumentOutOfRangeException(nameof(index));

        _lessonBlocks.Insert(index, block);
        ReorderSequences();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveLessonBlock(LessonBlock block)
    {
        if (block == null) throw new ArgumentNullException(nameof(block));

        if (_lessonBlocks.Remove(block))
        {
            ReorderSequences();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void MoveLessonBlock(Guid blockId, int newSequence)
    {
        var block = _lessonBlocks.FirstOrDefault(b => b.Id == blockId);
        if (block == null) throw new ArgumentException("Block not found in this lesson.", nameof(blockId));
        if (newSequence < 1 || newSequence > _lessonBlocks.Count) throw new ArgumentOutOfRangeException(nameof(newSequence));

        _lessonBlocks.Remove(block);
        _lessonBlocks.Insert(newSequence - 1, block);
        ReorderSequences();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceLessonBlock(Guid blockId, LessonBlock newBlock)
    {
        if (newBlock == null) throw new ArgumentNullException(nameof(newBlock));
        if (newBlock.LessonId != Id) throw new ArgumentException("New block does not belong to this lesson.", nameof(newBlock));

        var existingIndex = _lessonBlocks.FindIndex(b => b.Id == blockId);
        if (existingIndex == -1) throw new ArgumentException("Existing block not found.", nameof(blockId));

        newBlock.Sequence = _lessonBlocks[existingIndex].Sequence;
        _lessonBlocks[existingIndex] = newBlock;
        UpdatedAt = DateTime.UtcNow;
    }

    private void ReorderSequences()
    {
        for (int i = 0; i < _lessonBlocks.Count; i++)
        {
            _lessonBlocks[i].Sequence = i + 1;
        }
    }

    protected Lesson() : base()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Lesson(string title, string description, ReferenceLevel referenceLevel)
        : base()
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title.Trim();
        Description = description.Trim();
        ReferenceLevel = referenceLevel;
        Status = PublicationStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transitions the lesson to Draft status.
    /// </summary>
    public void SetDraft()
    {
        Status = PublicationStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transitions the lesson to Published status, making it visible in the catalog.
    /// </summary>
    public void Publish()
    {
        Status = PublicationStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Transitions the lesson to Disabled status, removing it from the catalog.
    /// </summary>
    public void Disable()
    {
        Status = PublicationStatus.Disabled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string description, ReferenceLevel referenceLevel)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Title = title.Trim();
        Description = description.Trim();
        ReferenceLevel = referenceLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearCategories()
    {
        _categories.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearLessonBlocks()
    {
        _lessonBlocks.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
