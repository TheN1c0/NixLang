using MediatR;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.Application.Users.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Create Email Value Object (this validates input format)
        var email = Email.Create(request.Email);

        // 2. Fetch user from repository
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        // 3. Verify password
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new InvalidCredentialsException();
        }

        // 4. Record login and save changes
        user.RecordLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Generate JWT token
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        // 6. Return minimal token response
        return new LoginResponse(token, expiresAt);
    }
}
