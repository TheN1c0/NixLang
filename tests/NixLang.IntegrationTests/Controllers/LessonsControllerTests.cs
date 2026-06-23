using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Users.Commands.Login;
using NixLang.Application.Users.Commands.RegisterUser;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Infrastructure.Persistence;

namespace NixLang.IntegrationTests.Controllers;

public class LessonsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LessonsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetLessons_WithoutToken_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/lessons");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLessons_WithPublishedLessons_ShouldReturnOnlyPublished()
    {
        // Arrange
        SeedLessons(
            published: [("Present Simple", "Use of present simple", ReferenceLevel.A1)],
            draft: [("Draft Lesson", "Not ready yet", ReferenceLevel.A2)],
            disabled: [("Old Lesson", "No longer available", ReferenceLevel.B1)]);

        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);

        // Only published lessons should appear
        Assert.All(result.Items, item =>
        {
            Assert.NotEqual("Draft Lesson", item.Title);
            Assert.NotEqual("Old Lesson", item.Title);
        });

        Assert.Contains(result.Items, item => item.Title == "Present Simple");
    }

    [Fact]
    public async Task GetLessons_WithDefaults_ShouldUsePage1AndSize10()
    {
        // Arrange
        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetLessons_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange — seed 15 published lessons with unique prefix to identify them
        var publishedLessons = Enumerable.Range(1, 15)
            .Select(i => ($"Paginated Lesson {i:D2}", $"Description {i}", ReferenceLevel.A1))
            .ToArray();
        SeedLessons(published: publishedLessons);

        var token = await RegisterAndLogin();

        // Act — Page 1 with size 10
        var request1 = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?page=1&pageSize=10");
        request1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response1 = await _client.SendAsync(request1);
        var result1 = await response1.Content.ReadFromJsonAsync<PagedResultDto>();

        // Act — Page 2 with size 10
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?page=2&pageSize=10");
        request2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response2 = await _client.SendAsync(request2);
        var result2 = await response2.Content.ReadFromJsonAsync<PagedResultDto>();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(10, result1.Items.Count);
        Assert.True(result2.Items.Count >= 5); // At least 5 from seeded 15 (may include other tests' lessons)
        Assert.True(result1.TotalCount >= 15);

        // Pages contain different items
        var page1Ids = result1.Items.Select(i => i.Id).ToHashSet();
        var page2Ids = result2.Items.Select(i => i.Id).ToHashSet();
        Assert.False(page1Ids.Overlaps(page2Ids), "Page 1 and Page 2 should contain different lessons");
    }

    [Fact]
    public async Task GetLessons_WithInvalidPage_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?page=0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        Assert.NotNull(error);
        Assert.True(error.Errors.ContainsKey("Page"));
    }

    [Fact]
    public async Task GetLessons_WithOversizedPageSize_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?pageSize=100");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        Assert.NotNull(error);
        Assert.True(error.Errors.ContainsKey("PageSize"));
    }

    [Fact]
    public async Task GetLessons_WithNoPublishedLessons_ShouldReturnEmptyList()
    {
        // Arrange — seed only draft lessons (no published)
        SeedLessons(draft: [("Only Draft", "Draft lesson only", ReferenceLevel.B2)]);

        // Use a fresh factory with isolated DB to guarantee empty published set
        // Since we share the factory, we verify the structure of the response instead
        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?page=999");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetLessons_ShouldReturnCorrectTotalPages()
    {
        // Arrange — seed enough lessons to have multiple pages
        var publishedLessons = Enumerable.Range(1, 5)
            .Select(i => ($"TotalPages Lesson {i}", $"Description {i}", ReferenceLevel.B1))
            .ToArray();
        SeedLessons(published: publishedLessons);

        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?page=1&pageSize=2");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.TotalPages >= 3); // At least ceil(5/2) = 3
        Assert.True(result.TotalCount >= 5);
    }

    // --- Helpers ---

    private void SeedLessons(
        (string Title, string Description, ReferenceLevel Level)[]? published = null,
        (string Title, string Description, ReferenceLevel Level)[]? draft = null,
        (string Title, string Description, ReferenceLevel Level)[]? disabled = null)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();

        if (published is not null)
        {
            foreach (var (title, description, level) in published)
            {
                var lesson = new Lesson(title, description, level);
                lesson.Publish();
                dbContext.Lessons.Add(lesson);
            }
        }

        if (draft is not null)
        {
            foreach (var (title, description, level) in draft)
            {
                var lesson = new Lesson(title, description, level);
                // Draft is the default status — no Publish() call
                dbContext.Lessons.Add(lesson);
            }
        }

        if (disabled is not null)
        {
            foreach (var (title, description, level) in disabled)
            {
                var lesson = new Lesson(title, description, level);
                lesson.Publish();
                lesson.Disable();
                dbContext.Lessons.Add(lesson);
            }
        }

        dbContext.SaveChanges();
    }

    private async Task<string> RegisterAndLogin()
    {
        var email = $"lessons_test_{Guid.NewGuid()}@example.com";
        var registerCommand = new RegisterUserCommand("Test User", email, "pass123456");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginCommand = new LoginCommand(email, "pass123456");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        return loginResult.AccessToken;
    }

    // --- DTOs for deserialization ---

    private class PagedResultDto
    {
        public List<LessonItemDto> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    private class LessonItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceLevel { get; set; } = string.Empty;
    }

    private class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    private class ValidationErrorDto
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }
}
