using NSubstitute;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Users.Commands.RegisterUser;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.UnitTests.Application.Users;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUserAndReturnId()
    {
        // Arrange
        var command = new RegisterUserCommand("Nico Test", "nico@example.com", "securePassword123");
        var email = Email.Create(command.Email);
        var passwordHash = "hashedPasswordHash";

        _userRepository.ExistsByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher.HashPassword(command.Password)
            .Returns(passwordHash);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.FullName == "Nico Test" && u.Email == email && u.PasswordHash == passwordHash),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldThrowEmailAlreadyExistsException()
    {
        // Arrange
        var command = new RegisterUserCommand("Nico Test", "nico@example.com", "securePassword123");
        var email = Email.Create(command.Email);

        _userRepository.ExistsByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<EmailAlreadyExistsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
