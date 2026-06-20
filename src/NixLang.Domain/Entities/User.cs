using NixLang.Domain.Common;
using NixLang.Domain.Enums;
using NixLang.Domain.ValueObjects;

namespace NixLang.Domain.Entities;

/// <summary>
/// Registered user of the NixLang platform.
/// Aggregate Root of the User Aggregate.
/// Maps to: Usuario.
/// Source: RN-01, RN-02, RN-03, RN-05.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected User() : base()
    {
        FullName = string.Empty;
        Email = null!;
        PasswordHash = string.Empty;
    }

    public User(string fullName, Email email, string passwordHash)
        : base()
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        if (email is null)
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        FullName = fullName.Trim();
        Email = email;
        PasswordHash = passwordHash;
        Role = UserRole.User;
        CreatedAt = DateTime.UtcNow;
    }
}
