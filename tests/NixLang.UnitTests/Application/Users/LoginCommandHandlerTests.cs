using NSubstitute;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Users.Commands.Login;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.UnitTests.Application.Users;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtTokenGenerator, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "securePassword123");
        var email = Email.Create(command.Email);
        var passwordHash = "hashedPassword";
        var user = new User("Test User", email, passwordHash);
        var expectedToken = "jwt_token_value";
        var expiryTime = DateTime.UtcNow.AddMinutes(60);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)
            .Returns(true);

        _jwtTokenGenerator.GenerateToken(user)
            .Returns((expectedToken, expiryTime));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.AccessToken);
        Assert.Equal(expiryTime, result.ExpiresAt);

        // Verify LastLoginAt is recorded and saved
        Assert.NotNull(user.LastLoginAt);
        Assert.True((DateTime.UtcNow - user.LastLoginAt.Value).TotalSeconds < 5);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "anyPassword");
        var email = Email.Create(command.Email);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _passwordHasher.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "wrongPassword");
        var email = Email.Create(command.Email);
        var passwordHash = "hashedPassword";
        var user = new User("Test User", email, passwordHash);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
