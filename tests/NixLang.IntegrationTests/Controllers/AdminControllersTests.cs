using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Categories.Commands.CreateCategory;
using NixLang.Application.Categories.Commands.UpdateCategory;
using NixLang.Application.Categories.Queries.GetCategories;
using NixLang.Application.Common.Models;
using NixLang.Application.Exercises.Commands.CreateExercise;
using NixLang.Application.Exercises.Commands.UpdateExercise;
using NixLang.Application.Exercises.Queries.GetExercises;
using NixLang.Application.Lessons.Commands.CreateLesson;
using NixLang.Application.Lessons.Commands.UpdateLesson;
using NixLang.Application.Lessons.Queries.GetAdminLessons;
using NixLang.Application.Users.Commands.Login;
using NixLang.Application.Users.Commands.RegisterUser;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Infrastructure.Persistence;
using Xunit;

namespace NixLang.IntegrationTests.Controllers;

public class AdminControllersTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminControllersTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AdminEndpoints_ShouldReturn403Forbidden_ForNonAdminUser()
    {
        // Arrange: standard student user
        var studentToken = await RegisterAndLogin("Student User", $"student_{Guid.NewGuid()}@example.com", "pass123456");
        
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/categories");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CategoryCrud_WithAdminUser_ShouldWorkSuccessfully()
    {
        // Arrange
        var adminToken = await CreateAdminAndLogin();
        
        // 1. Create Category
        var createCommand = new CreateCategoryCommand("Prepositions", "Prepositions of time and place");
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/admin/categories")
        {
            Content = JsonContent.Create(createCommand)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createResult = await createResponse.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(createResult);
        var categoryId = createResult.Id;

        // 2. Get Categories
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/admin/categories");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await _client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var categories = await getResponse.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.NotNull(categories);
        Assert.Contains(categories, c => c.Id == categoryId && c.Name == "Prepositions");

        // 3. Update Category
        var updateCommand = new UpdateCategoryCommand(categoryId, "Prepositions Updated", "New description");
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/categories/{categoryId}")
        {
            Content = JsonContent.Create(updateCommand)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // 4. Delete Category
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/categories/{categoryId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task ExerciseCrud_AndUsageIntegrity_ShouldWorkSuccessfully()
    {
        // Arrange
        var adminToken = await CreateAdminAndLogin();
        var statement = $"Question_{Guid.NewGuid()}";

        // 1. Create Exercise
        var createCommand = new CreateExerciseCommand(
            "MultipleChoice",
            statement,
            null,
            null,
            new List<CreateExerciseOptionDto>
            {
                new CreateExerciseOptionDto("Option A", true, 1),
                new CreateExerciseOptionDto("Option B", false, 2)
            });

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/admin/exercises")
        {
            Content = JsonContent.Create(createCommand)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createResult = await createResponse.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(createResult);
        var exerciseId = createResult.Id;

        // 2. Update Exercise
        var updateCommand = new UpdateExerciseCommand(
            exerciseId,
            "MultipleChoice",
            statement + " Updated",
            null,
            null,
            new List<CreateExerciseOptionDto>
            {
                new CreateExerciseOptionDto("Option A Updated", true, 1),
                new CreateExerciseOptionDto("Option B Updated", false, 2)
            });

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/exercises/{exerciseId}")
        {
            Content = JsonContent.Create(updateCommand)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        if (updateResponse.StatusCode != HttpStatusCode.OK)
        {
            var body = await updateResponse.Content.ReadAsStringAsync();
            throw new Exception($"Update exercise failed with status {updateResponse.StatusCode} and body {body}");
        }

        // 3. Associate with Lesson to verify Delete integrity
        var lessonId = Guid.Empty;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var lesson = new Lesson("Temporary Lesson", "For testing integrity", ReferenceLevel.B1);
            lessonId = lesson.Id;
            var block = LessonBlock.CreateExerciseBlock(lessonId, 1, exerciseId);
            lesson.AddLessonBlock(block);

            dbContext.Lessons.Add(lesson);
            await dbContext.SaveChangesAsync();
        }

        // 4. Try deleting the exercise (should fail with 409 Conflict because it's in use)
        var deleteInUseRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/exercises/{exerciseId}");
        deleteInUseRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteInUseResponse = await _client.SendAsync(deleteInUseRequest);
        Assert.Equal(HttpStatusCode.Conflict, deleteInUseResponse.StatusCode);

        // 5. Clean up Lesson (Delete Lesson) so exercise is no longer in use
        var deleteLessonRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/lessons/{lessonId}");
        deleteLessonRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteLessonResponse = await _client.SendAsync(deleteLessonRequest);
        Assert.Equal(HttpStatusCode.OK, deleteLessonResponse.StatusCode);

        // 6. Delete Exercise (should now succeed)
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/exercises/{exerciseId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task LessonCrud_ShouldWorkSuccessfully()
    {
        // Arrange
        var adminToken = await CreateAdminAndLogin();

        // 1. Create Exercise for the lesson
        var createExCommand = new CreateExerciseCommand(
            "MultipleChoice",
            "What is the capital of England?",
            "London",
            null,
            new List<CreateExerciseOptionDto>
            {
                new CreateExerciseOptionDto("London", true, 1),
                new CreateExerciseOptionDto("Manchester", false, 2)
            });
        var createExRequest = new HttpRequestMessage(HttpMethod.Post, "/api/admin/exercises")
        {
            Content = JsonContent.Create(createExCommand)
        };
        createExRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createExResponse = await _client.SendAsync(createExRequest);
        var exerciseId = (await createExResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // 2. Create Lesson with Heading and Exercise block
        var createCommand = new CreateLessonCommand(
            "Admin Lesson",
            "Admin Lesson Description",
            "B2",
            null,
            new List<CreateLessonBlockDto>
            {
                new CreateLessonBlockDto("Heading", "Introduction", null),
                new CreateLessonBlockDto("Exercise", string.Empty, exerciseId)
            });

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/admin/lessons")
        {
            Content = JsonContent.Create(createCommand)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createResult = await createResponse.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(createResult);
        var lessonId = createResult.Id;

        // 3. Get Lessons
        var getRequest = new HttpRequestMessage(HttpMethod.Get, "/api/admin/lessons");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await _client.SendAsync(getRequest);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var lessons = await getResponse.Content.ReadFromJsonAsync<PagedResult<AdminLessonSummaryDto>>();
        Assert.NotNull(lessons);
        Assert.Contains(lessons.Items, l => l.Id == lessonId && l.Title == "Admin Lesson");

        // 4. Update Lesson
        var updateCommand = new UpdateLessonCommand(
            lessonId,
            "Admin Lesson Updated",
            "Admin Lesson Description Updated",
            "B2",
            "Published",
            null,
            new List<CreateLessonBlockDto>
            {
                new CreateLessonBlockDto("Heading", "Introduction Updated", null),
                new CreateLessonBlockDto("Exercise", string.Empty, exerciseId)
            });

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/lessons/{lessonId}")
        {
            Content = JsonContent.Create(updateCommand)
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var updateResponse = await _client.SendAsync(updateRequest);
        if (updateResponse.StatusCode != HttpStatusCode.OK)
        {
            var body = await updateResponse.Content.ReadAsStringAsync();
            throw new Exception($"Update lesson failed with status {updateResponse.StatusCode} and body {body}");
        }

        // 4. Delete Lesson
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/lessons/{lessonId}");
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteResponse = await _client.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }

    private async Task<string> CreateAdminAndLogin()
    {
        var emailStr = $"admin_{Guid.NewGuid()}@example.com";
        var password = "adminPassword123";

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

        var loginCommand = new LoginCommand(emailStr, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        return loginResult.AccessToken;
    }

    private async Task<string> RegisterAndLogin(string fullName, string email, string password)
    {
        var registerCommand = new RegisterUserCommand(fullName, email, password);
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginCommand = new LoginCommand(email, password);
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        return loginResult.AccessToken;
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }

    private class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
