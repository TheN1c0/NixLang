using System;

namespace NixLang.Application.Common.Exceptions;

public class LessonNotFoundException : Exception
{
    public LessonNotFoundException(Guid lessonId)
        : base($"Lesson with ID '{lessonId}' was not found or is not published.")
    {
    }
}
