using System.Text.RegularExpressions;

namespace NixLang.Domain.ValueObjects;

/// <summary>
/// Represents a validated email address.
/// Maps to: CorreoElectrónico — validated string with email format, unique in the system.
/// Source: RN-01.
/// </summary>
public sealed partial class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        var trimmed = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(trimmed))
            throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));

        return new Email(trimmed);
    }

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Value;

    public static bool operator ==(Email? left, Email? right) => Equals(left, right);

    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
