using System;

namespace NixLang.Application.Common.Exceptions;

public class CollectionNotFoundException : Exception
{
    public CollectionNotFoundException(Guid collectionId)
        : base($"Collection with ID '{collectionId}' was not found.")
    {
    }
}
