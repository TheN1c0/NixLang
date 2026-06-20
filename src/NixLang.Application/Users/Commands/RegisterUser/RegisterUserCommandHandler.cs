using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the Email Value Object (this validates the email format Regex and trims/lowercases it)
        var email = Email.Create(request.Email);

        // 2. Check for duplicate email (RN-01)
        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new EmailAlreadyExistsException(email.Value);
        }

        // 3. Hash the password (RNF-012)
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 4. Create the User entity (RN-02 defaults role to User)
        var user = new User(request.FullName, email, passwordHash);

        // 5. Persist the user
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Return the created user's ID
        return user.Id;
    }
}
