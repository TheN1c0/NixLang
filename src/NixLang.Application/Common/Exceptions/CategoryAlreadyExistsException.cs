namespace NixLang.Application.Common.Exceptions;

public class CategoryAlreadyExistsException : Exception
{
    public CategoryAlreadyExistsException(string name)
        : base($"The category '{name}' already exists.")
    {
    }
}
