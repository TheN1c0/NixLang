namespace NixLang.Domain.Enums;

/// <summary>
/// Types of exercises available in a lesson.
/// Maps to: TipoEjercicio (TRADUCCION, COMPLETAR_ESPACIOS, OPCION_MULTIPLE, PRONUNCIACION).
/// Source: RF-015 to RF-018.
/// </summary>
public enum ExerciseType
{
    Translation = 1,
    FillInTheBlank = 2,
    MultipleChoice = 3,
    Pronunciation = 4
}
