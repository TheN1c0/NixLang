namespace NixLang.Application.Users.Queries.GetUserProfile;

/// <summary>
/// DTO representing the user's profile data.
/// Maps from the User entity for external consumption.
/// </summary>
public record UserProfileResponse(Guid Id, string FullName, string Email, string Role);
