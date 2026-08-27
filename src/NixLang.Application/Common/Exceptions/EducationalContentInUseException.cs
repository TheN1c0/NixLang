using System;

namespace NixLang.Application.Common.Exceptions;

public class EducationalContentInUseException : Exception
{
    public EducationalContentInUseException(Guid id)
        : base($"Educational content with ID '{id}' cannot be deleted because it is currently referenced by one or more lesson blocks.")
    {
    }
}
