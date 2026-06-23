using MediatR;

namespace NixLang.Application.Users.Queries.GetUserProfile;

/// <summary>
/// Query to retrieve the profile of the currently authenticated user.
/// No parameters required — the user is identified via ICurrentUserService.
/// </summary>
public record GetUserProfileQuery() : IRequest<UserProfileResponse>;
