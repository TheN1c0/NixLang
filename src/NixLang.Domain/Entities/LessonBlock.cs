using System;
using NixLang.Domain.Common;
using NixLang.Domain.Enums;
using NixLang.Domain.ValueObjects;

namespace NixLang.Domain.Entities;

public class LessonBlock : BaseEntity
{
    public Guid LessonId { get; private set; }
    public LessonBlockType Type { get; private set; }
    public int Sequence { get; internal set; }
    public BlockConfiguration Configuration { get; private set; }
    public Guid? ReferencedExerciseId { get; private set; }
    public Exercise? Exercise { get; private set; }

    protected LessonBlock() : base()
    {
        Configuration = new BlockConfiguration(string.Empty);
    }

    private LessonBlock(Guid lessonId, LessonBlockType type, int sequence, BlockConfiguration configuration, Guid? referencedExerciseId)
        : base()
    {
        if (lessonId == Guid.Empty)
            throw new ArgumentException("Lesson ID cannot be empty.", nameof(lessonId));

        if (sequence < 1)
            throw new ArgumentOutOfRangeException(nameof(sequence), "Sequence must be at least 1.");

        LessonId = lessonId;
        Type = type;
        Sequence = sequence;
        Configuration = configuration ?? new BlockConfiguration(string.Empty);
        ReferencedExerciseId = referencedExerciseId;
    }

    public static LessonBlock CreateExerciseBlock(Guid lessonId, int sequence, Guid referencedExerciseId)
    {
        if (referencedExerciseId == Guid.Empty)
            throw new ArgumentException("Referenced exercise ID is required.", nameof(referencedExerciseId));

        return new LessonBlock(lessonId, LessonBlockType.Exercise, sequence, new BlockConfiguration(string.Empty), referencedExerciseId);
    }

    public static LessonBlock CreateInformationalBlock(Guid lessonId, LessonBlockType type, int sequence, BlockConfiguration configuration)
    {
        if (type == LessonBlockType.Exercise)
            throw new ArgumentException("Use CreateExerciseBlock to instantiate exercise blocks.", nameof(type));

        return new LessonBlock(lessonId, type, sequence, configuration, null);
    }
}
