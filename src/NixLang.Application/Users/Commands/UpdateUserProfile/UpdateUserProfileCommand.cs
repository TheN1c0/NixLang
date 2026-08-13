using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Application.Users.Commands.UpdateUserProfile;

public record UpdateUserProfileCommand(
    string FullName,
    string Email,
    string? CurrentPassword,
    string? NewPassword) : IRequest<bool>;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Fetch user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }

        // 2. Validate email format & unique
        var email = Email.Create(request.Email);
        if (user.Email.Value.ToLower() != email.Value.ToLower())
        {
            if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
            {
                throw new EmailAlreadyExistsException(email.Value);
            }
        }

        // 3. Update profile details
        user.UpdateProfile(request.FullName, email);

        // 4. Update password if requested
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || 
                !_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            var hashedNew = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatePassword(hashedNew);
        }

        // 5. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
