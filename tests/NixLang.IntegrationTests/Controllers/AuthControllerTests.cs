using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
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
}
