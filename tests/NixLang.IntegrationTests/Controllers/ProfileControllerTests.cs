using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Users.Commands.Login;
using NixLang.Application.Users.Commands.RegisterUser;
using NixLang.Application.Users.Commands.DeleteAccount;
using NixLang.Infrastructure.Persistence;

namespace NixLang.IntegrationTests.Controllers;

public class ProfileControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProfileControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/profile");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithValidToken_ShouldReturnUserProfile()
    {
        // Arrange
        var fullName = "Juan Pérez";
        var email = $"profile_test_{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLogin(fullName, email, "pass123456");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<ProfileResponseDto>();
        Assert.NotNull(profile);
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(fullName, profile.FullName);
        Assert.Equal(email.ToLowerInvariant(), profile.Email);
        Assert.Equal("User", profile.Role);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnCorrectEmail()
    {
        // Arrange
        var email = $"Email_CASE_{Guid.NewGuid()}@Example.COM";
        var token = await RegisterAndLogin("Test User", email, "pass123456");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<ProfileResponseDto>();
        Assert.NotNull(profile);
        Assert.Equal(email.ToLowerInvariant(), profile.Email);
    }

    [Fact]
    public async Task GetProfile_WithAdminUser_ShouldReturnAdministratorRole()
    {
        // Arrange
        var emailStr = $"admin_profile_{Guid.NewGuid()}@example.com";
        var password = "adminPassword123";

        // Manually seed the Admin user into database
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<NixLang.Application.Common.Interfaces.IPasswordHasher>();

            var email = NixLang.Domain.ValueObjects.Email.Create(emailStr);
            var passwordHash = passwordHasher.HashPassword(password);

            var adminUser = new NixLang.Domain.Entities.User("System Admin", email, passwordHash);
            adminUser.UpdateRole(NixLang.Domain.Enums.UserRole.Administrator);

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
        }

        // Login as admin
        var loginCommand = new LoginCommand(emailStr, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<ProfileResponseDto>();
        Assert.NotNull(profile);
        Assert.Equal("Administrator", profile.Role);
    }

    [Fact]
    public async Task GetProfile_WithInvalidToken_ShouldReturn401()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProfile_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var deleteCommand = new DeleteAccountCommand("somePassword");
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/profile")
        {
            Content = JsonContent.Create(deleteCommand)
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProfile_WithWrongPassword_ShouldReturn401()
    {
        // Arrange
        var email = $"delete_wrong_pass_{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLogin("User Delete", email, "correctPassword123");

        var deleteCommand = new DeleteAccountCommand("wrongPassword999");
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/profile")
        {
            Content = JsonContent.Create(deleteCommand)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProfile_WithValidPassword_ShouldDeleteUserSuccessfully()
    {
        // Arrange
        var email = $"delete_success_{Guid.NewGuid()}@example.com";
        var password = "validPassword123";
        var token = await RegisterAndLogin("User To Delete", email, password);

        var deleteCommand = new DeleteAccountCommand(password);
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/profile")
        {
            Content = JsonContent.Create(deleteCommand)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify that user can no longer log in
        var loginCommand = new LoginCommand(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    /// <summary>
    /// Helper: Registers a user, logs in, and returns the JWT access token.
    /// </summary>
    private async Task<string> RegisterAndLogin(string fullName, string email, string password)
    {
        var registerCommand = new RegisterUserCommand(fullName, email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginCommand = new LoginCommand(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        return loginResult.AccessToken;
    }

    private class ProfileResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    private class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
