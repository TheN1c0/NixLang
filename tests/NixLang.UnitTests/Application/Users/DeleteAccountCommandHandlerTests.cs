using NSubstitute;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Users.Commands.DeleteAccount;
using NixLang.Domain.Entities;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.UnitTests.Application.Users;

public class DeleteAccountCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new DeleteAccountCommandHandler(
            _userRepository,
            _passwordHasher,
            _currentUserService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidPassword_ShouldDeleteUserAndSaveChanges()
    {
        // Arrange
        var email = Email.Create("user@test.com");
        var user = new User("User Test", email, "hashedPassword123");
        var userId = user.Id;

        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("mySecretPass", "hashedPassword123").Returns(true);

        var command = new DeleteAccountCommand("mySecretPass");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        await _userRepository.Received(1).DeleteAsync(user, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new DeleteAccountCommand("mySecretPass");

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

        await _userRepository.DidNotReceive().DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var email = Email.Create("user@test.com");
        var user = new User("User Test", email, "hashedPassword123");
        var userId = user.Id;

        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("wrongPass", "hashedPassword123").Returns(false);

        var command = new DeleteAccountCommand("wrongPass");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        await _userRepository.DidNotReceive().DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var email = Email.Create("user@test.com");
        var user = new User("User Test", email, "hashedPassword123");
        var userId = user.Id;

        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var command = new DeleteAccountCommand("");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(command, CancellationToken.None));

        await _userRepository.DidNotReceive().DeleteAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
