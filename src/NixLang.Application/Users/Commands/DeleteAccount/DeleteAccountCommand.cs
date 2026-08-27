using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NixLang.Application.Users.Commands.DeleteAccount;

public record DeleteAccountCommand(string Password) : IRequest<bool>;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(
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

    public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // 1. Fetch user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }

        // 2. Verify password for security confirmation
        if (string.IsNullOrWhiteSpace(request.Password) || 
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        // 3. Delete user entity (cascade deletes dependent records in DB)
        await _userRepository.DeleteAsync(user, cancellationToken);

        // 4. Commit transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
