namespace NixLang.Application.Common.Exceptions;

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(Guid id)
        : base($"The category with ID '{id}' was not found.")
    {
    }
}
