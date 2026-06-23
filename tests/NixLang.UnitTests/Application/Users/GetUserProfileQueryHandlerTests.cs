using NSubstitute;
using NixLang.Application.Common.Exceptions;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Users.Queries.GetUserProfile;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.Repositories;
using NixLang.Domain.ValueObjects;

namespace NixLang.UnitTests.Application.Users;

public class GetUserProfileQueryHandlerTests
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly GetUserProfileQueryHandler _handler;

    public GetUserProfileQueryHandlerTests()
    {
        _currentUserService = Substitute.For<ICurrentUserService>();
        _userRepository = Substitute.For<IUserRepository>();
        _handler = new GetUserProfileQueryHandler(_currentUserService, _userRepository);
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldReturnUserProfileResponse()
    {
        // Arrange
        var email = Email.Create("juan@test.com");
        var user = new User("Juan Pérez", email, "hashedPassword");
        var userId = user.Id;

        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(new GetUserProfileQuery(), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Juan Pérez", result.FullName);
        Assert.Equal("juan@test.com", result.Email);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldThrowUserNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _handler.Handle(new GetUserProfileQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User("Test User", email, "hashedPassword");
        var userId = user.Id;

        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        await _handler.Handle(new GetUserProfileQuery(), CancellationToken.None);

        // Assert
        await _userRepository.Received(1).GetByIdAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAdministratorRole_ShouldReturnAdministratorRole()
    {
        // Arrange
        var email = Email.Create("admin@test.com");
        var user = new User("Admin User", email, "hashedPassword");
        user.UpdateRole(UserRole.Administrator);
        var userId = user.Id;

        _currentUserService.UserId.Returns(userId);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _handler.Handle(new GetUserProfileQuery(), CancellationToken.None);

        // Assert
        Assert.Equal("Administrator", result.Role);
    }
}
