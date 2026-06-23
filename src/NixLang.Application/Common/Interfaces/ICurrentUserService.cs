namespace NixLang.Application.Common.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's identity.
/// The implementation extracts user information from the JWT claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier (GUID) of the currently authenticated user.
    /// Extracted from the "sub" claim of the JWT token.
    /// </summary>
    Guid UserId { get; }
}
