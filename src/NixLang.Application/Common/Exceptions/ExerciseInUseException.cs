namespace NixLang.Application.Common.Exceptions;

public class ExerciseInUseException : Exception
{
    public ExerciseInUseException(Guid id)
        : base($"The exercise with ID '{id}' cannot be deleted because it is currently used in one or more lesson blocks.")
    {
    }
}
