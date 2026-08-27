using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Common.Models;
using NixLang.Application.EducationalContents.Commands.CreateEducationalContent;
using NixLang.Application.EducationalContents.Commands.UpdateEducationalContent;
using NixLang.Application.EducationalContents.Queries.GetAdminEducationalContents;
using NixLang.Application.EducationalContents.Queries.GetEducationalContentById;
using NixLang.Application.EducationalContents.Queries.GetEducationalContents;
using NixLang.Application.Exercises.Commands.CreateExercise;
using NixLang.Application.Lessons.Commands.CreateLesson;
using NixLang.Application.Lessons.Queries.GetLessonById;
using NixLang.Application.Users.Commands.Login;
using NixLang.Application.Users.Commands.RegisterUser;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Infrastructure.Persistence;
using Xunit;

namespace NixLang.IntegrationTests.Controllers;

public class EducationalContentControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EducationalContentControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task EducationalContent_PublicAndAdminFlow_ShouldWorkSeamlessly()
    {
        // 1. Arrange admin & student tokens
        var adminToken = await CreateAdminAndLogin();
        var studentToken = await RegisterAndLogin("Student Content Reader", $"student_{Guid.NewGuid()}@example.com", "Password123!");

        // 2. Admin creates EducationalContent in Draft
        var createCmd = new CreateEducationalContentCommand(
            "False Friends: Actually vs Currently",
            "Learn the common trap of 'actually'",
            "Actually means 'in fact', not 'at present'.",
            "CommonMistake",
            "A2",
            "Draft");

        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/admin/content")
        {
            Content = JsonContent.Create(createCmd)
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createRes = await _client.SendAsync(createReq);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var createdResult = await createRes.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(createdResult);
        var contentId = createdResult.Id;

        // 3. Student querying public /api/content should NOT see Draft content
        var studentListReq = new HttpRequestMessage(HttpMethod.Get, "/api/content?search=Actually");
        studentListReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
        var studentListRes = await _client.SendAsync(studentListReq);
        Assert.Equal(HttpStatusCode.OK, studentListRes.StatusCode);
        var pagedContent = await studentListRes.Content.ReadFromJsonAsync<PagedResult<EducationalContentItemDto>>();
        Assert.NotNull(pagedContent);
        Assert.DoesNotContain(pagedContent.Items, c => c.Id == contentId);

        // 4. Admin updates and Publishes the content
        var updateCmd = new UpdateEducationalContentCommand(
            contentId,
            "False Friends: Actually vs Currently (Updated)",
            "Learn the common trap of 'actually'",
            "Actually means 'in fact', not 'at present'. Remember to use 'currently' for current time.",
            "CommonMistake",
            "A2",
            "Published");

        var updateReq = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/content/{contentId}")
        {
            Content = JsonContent.Create(updateCmd)
        };
        updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var updateRes = await _client.SendAsync(updateReq);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        // 5. Student querying public /api/content can now find it and get detail
        var studentPubReq = new HttpRequestMessage(HttpMethod.Get, $"/api/content/{contentId}");
        studentPubReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
        var studentPubRes = await _client.SendAsync(studentPubReq);
        Assert.Equal(HttpStatusCode.OK, studentPubRes.StatusCode);
        var contentDetail = await studentPubRes.Content.ReadFromJsonAsync<EducationalContentDetailDto>();
        Assert.NotNull(contentDetail);
        Assert.Equal("False Friends: Actually vs Currently (Updated)", contentDetail.Title);
        Assert.Equal("CommonMistake", contentDetail.Type);

        // 6. Admin creates a Lesson referencing this Content block and an Exercise block
        // First, create an exercise
        var createExCmd = new CreateExerciseCommand(
            "MultipleChoice",
            "Choose the right word: 'I ____ work remotely.'",
            "currently",
            null,
            new List<CreateExerciseOptionDto>
            {
                new CreateExerciseOptionDto("currently", true, 1),
                new CreateExerciseOptionDto("actually", false, 2)
            });
        var createExReq = new HttpRequestMessage(HttpMethod.Post, "/api/admin/exercises")
        {
            Content = JsonContent.Create(createExCmd)
        };
        createExReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createExRes = await _client.SendAsync(createExReq);
        var exId = (await createExRes.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // Create lesson with Content block + Exercise block
        var createLessonCmd = new CreateLessonCommand(
            "False Friends Mastery",
            "Master false friends in English",
            "A2",
            null,
            new List<CreateLessonBlockDto>
            {
                new CreateLessonBlockDto("Heading", "Introduction to False Friends", null),
                new CreateLessonBlockDto("Content", string.Empty, null, contentId),
                new CreateLessonBlockDto("Exercise", string.Empty, exId)
            });

        var createLessonReq = new HttpRequestMessage(HttpMethod.Post, "/api/admin/lessons")
        {
            Content = JsonContent.Create(createLessonCmd)
        };
        createLessonReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createLessonRes = await _client.SendAsync(createLessonReq);
        Assert.Equal(HttpStatusCode.Created, createLessonRes.StatusCode);
        var lessonId = (await createLessonRes.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // 7. Verify that trying to delete EducationalContent while referenced in a lesson block fails (409 Conflict)
        var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/content/{contentId}");
        deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteRes = await _client.SendAsync(deleteReq);
        Assert.Equal(HttpStatusCode.Conflict, deleteRes.StatusCode);
    }

    private async Task<string> CreateAdminAndLogin()
    {
        var email = $"admin_{Guid.NewGuid()}@example.com";
        var password = "AdminPassword123!";

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
        var user = new User("Administrator", NixLang.Domain.ValueObjects.Email.Create(email), BCrypt.Net.BCrypt.HashPassword(password));
        user.UpdateRole(UserRole.Administrator);
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, password));
        var authRes = await loginRes.Content.ReadFromJsonAsync<LoginResponseDto>();
        return authRes!.AccessToken;
    }

    private async Task<string> RegisterAndLogin(string name, string email, string password)
    {
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterUserCommand(name, email, password));
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginCommand(email, password));
        var authRes = await loginRes.Content.ReadFromJsonAsync<LoginResponseDto>();
        return authRes!.AccessToken;
    }

    private class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }
}
