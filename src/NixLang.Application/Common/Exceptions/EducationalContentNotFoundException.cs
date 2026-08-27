using System;

namespace NixLang.Application.Common.Exceptions;

public class EducationalContentNotFoundException : Exception
{
    public EducationalContentNotFoundException(Guid id)
        : base($"Educational content with ID '{id}' was not found.")
    {
    }
}
