using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Users.Commands.Login;
using NixLang.Application.Users.Commands.RegisterUser;
using NixLang.Domain.Enums;
using NixLang.Infrastructure.Persistence;

namespace NixLang.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidRequest_ShouldCreateUser()
    {
        // Arrange
        var uniqueEmail = $"integration_{Guid.NewGuid()}@example.com";
        var command = new RegisterUserCommand("Juan Perez", uniqueEmail, "pass123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with status {response.StatusCode} and content: {content}");
        }

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.UserId);

        // Verify database state
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
        var userInDb = await dbContext.Users.FindAsync(result.UserId);

        Assert.NotNull(userInDb);
        Assert.Equal("Juan Perez", userInDb.FullName);
        Assert.Equal(uniqueEmail.ToLowerInvariant(), userInDb.Email.Value);
        Assert.NotEqual("pass123456", userInDb.PasswordHash); // Password must be hashed (RNF-012)
        Assert.Equal(UserRole.User, userInDb.Role); // Default role (RN-02)
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var duplicateEmail = $"duplicate_{Guid.NewGuid()}@example.com";
        var firstCommand = new RegisterUserCommand("Juan Perez", duplicateEmail, "pass123456");
        var secondCommand = new RegisterUserCommand("Juan Perez Two", duplicateEmail, "otherPass123");

        // Act - First Register
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", firstCommand);
        if (firstResponse.StatusCode != HttpStatusCode.Created)
        {
            var content = await firstResponse.Content.ReadAsStringAsync();
            throw new Exception($"First request failed with status {firstResponse.StatusCode} and content: {content}");
        }
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act - Second Register with same email
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", secondCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Conflict", error.Title);
        Assert.Contains(duplicateEmail, error.Detail);
    }

    [Theory]
    [InlineData("", "valid@example.com", "pass123456")] // Empty name
    [InlineData("Juan Perez", "invalid-email", "pass123456")] // Invalid email format
    [InlineData("Juan Perez", "valid@example.com", "short")] // Password too short
    public async Task Register_WithInvalidData_ShouldReturnBadRequest(string name, string email, string password)
    {
        // Arrange
        var command = new RegisterUserCommand(name, email, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("One or more validation errors occurred.", error.Title);
        Assert.True(error.Errors.Count > 0);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnJwt()
    {
        // Arrange
        var email = $"login_test_{Guid.NewGuid()}@example.com";
        var registerCommand = new RegisterUserCommand("Juan Perez", email, "pass123456");

        // Pre-register user
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginCommand = new LoginCommand(email, "pass123456");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);
        Assert.NotEmpty(loginResult.AccessToken);
        Assert.True(loginResult.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        var email = $"login_fail_{Guid.NewGuid()}@example.com";
        var registerCommand = new RegisterUserCommand("Juan Perez", email, "pass123456");

        // Pre-register user
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var wrongPasswordCommand = new LoginCommand(email, "wrongPassword123");
        var wrongEmailCommand = new LoginCommand("wrong@email.com", "pass123456");

        // Act & Assert - Incorrect Password
        var passwordResponse = await _client.PostAsJsonAsync("/api/auth/login", wrongPasswordCommand);
        Assert.Equal(HttpStatusCode.Unauthorized, passwordResponse.StatusCode);

        var passwordError = await passwordResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(passwordError);
        Assert.Equal("Unauthorized", passwordError.Title);
        Assert.Equal("Invalid credentials.", passwordError.Detail);

        // Act & Assert - Non-existent Email
        var emailResponse = await _client.PostAsJsonAsync("/api/auth/login", wrongEmailCommand);
        Assert.Equal(HttpStatusCode.Unauthorized, emailResponse.StatusCode);

        var emailError = await emailResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(emailError);
        Assert.Equal("Unauthorized", emailError.Title);
        Assert.Equal("Invalid credentials.", emailError.Detail);
    }

    [Theory]
    [InlineData("invalid-email", "pass123456")] // Invalid email format
    [InlineData("", "pass123456")] // Empty email
    [InlineData("valid@example.com", "")] // Empty password
    public async Task Login_WithInvalidFormat_ShouldReturnBadRequest(string email, string password)
    {
        // Arrange
        var command = new LoginCommand(email, password);

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("One or more validation errors occurred.", error.Title);
        Assert.True(error.Errors.Count > 0);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/testauth/authenticated");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ShouldReturn200()
    {
        // Arrange
        var email = $"auth_user_{Guid.NewGuid()}@example.com";
        var registerCommand = new RegisterUserCommand("Standard User", email, "pass123456");
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand(email, "pass123456");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/testauth/authenticated");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithStandardUserToken_ShouldReturn403()
    {
        // Arrange
        var email = $"standard_user_{Guid.NewGuid()}@example.com";
        var registerCommand = new RegisterUserCommand("Standard User", email, "pass123456");
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginCommand(email, "pass123456");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/testauth/admin");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminToken_ShouldReturn200()
    {
        // Arrange
        var emailStr = $"admin_{Guid.NewGuid()}@example.com";
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

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/testauth/admin");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private class RegisterResponse
    {
        public Guid UserId { get; set; }
    }

    private class ErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
    }

    private class ValidationErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }

    private class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
