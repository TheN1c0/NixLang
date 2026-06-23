using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;

namespace NixLang.Application.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponse> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        // 1. Obtain the authenticated user's ID from the JWT claims
        var userId = _currentUserService.UserId;

        // 2. Fetch user from repository
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        // 3. Map to response DTO
        return new UserProfileResponse(
            user.Id,
            user.FullName,
            user.Email.Value,
            user.Role.ToString());
    }
}
