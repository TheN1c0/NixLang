namespace NixLang.Domain.Enums;

/// <summary>
/// Publication status for lessons.
/// Maps to: EstadoPublicación (borrador, publicada, desactivada).
/// Source: RN-11, HU-133 to HU-136, RF-046.
/// </summary>
public enum PublicationStatus
{
    Draft = 1,
    Published = 2,
    Disabled = 3
}
