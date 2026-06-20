namespace NixLang.Domain.Enums;

/// <summary>
/// Status of a user's progress within a lesson attempt.
/// Maps to: EstadoProgreso (NO_INICIADA, EN_PROGRESO, COMPLETADA).
/// Source: RN-26.
/// </summary>
public enum ProgressStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3
}
