namespace NixLang.Application.Common.Exceptions;

public class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email)
        : base($"The email '{email}' is already registered in the system.")
    {
    }
}
