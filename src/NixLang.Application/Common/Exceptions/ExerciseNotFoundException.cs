namespace NixLang.Application.Common.Exceptions;

public class ExerciseNotFoundException : Exception
{
    public ExerciseNotFoundException(Guid id)
        : base($"The exercise with ID '{id}' was not found.")
    {
    }
}
