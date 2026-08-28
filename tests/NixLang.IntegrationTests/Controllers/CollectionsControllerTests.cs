using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Collections.Commands.CreateCollection;
using NixLang.Application.Collections.Commands.UpdateCollection;
using NixLang.Application.Collections.Queries.GetAdminCollectionById;
using NixLang.Application.Collections.Queries.GetAdminCollections;
using NixLang.Application.Collections.Queries.GetCollectionById;
using NixLang.Application.Collections.Queries.GetCollections;
using NixLang.Application.Common.Models;
using NixLang.Application.Users.Commands.Login;
using NixLang.Application.Users.Commands.RegisterUser;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Infrastructure.Persistence;
using Xunit;

namespace NixLang.IntegrationTests.Controllers;

public class CollectionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CollectionsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task StudentEndpoints_ShouldRequireAuthentication()
    {
        var response = await _client.GetAsync("/api/collections");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoints_ShouldReturn403Forbidden_ForStandardStudent()
    {
        var studentToken = await RegisterAndLogin("Standard Student", $"student_{Guid.NewGuid()}@example.com", "pass123456");

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/collections");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task FullCollectionLifecycle_AdminAndStudent_ShouldWorkCorrectly()
    {
        // 1. Arrange Admin and Student
        var adminToken = await CreateAdminAndLogin();
        var studentToken = await RegisterAndLogin("Student Explorer", $"explorer_{Guid.NewGuid()}@example.com", "pass123456");

        // 2. Create 2 published lessons directly in DbContext for testing
        Guid lessonId1;
        Guid lessonId2;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var l1 = new Lesson("Airport Basics", "Vocabulary for checking in at airport", ReferenceLevel.A1);
            l1.Publish();
            var l2 = new Lesson("Booking a Room", "Vocabulary for hotel booking", ReferenceLevel.A2);
            l2.Publish();

            db.Lessons.AddRange(l1, l2);
            await db.SaveChangesAsync();

            lessonId1 = l1.Id;
            lessonId2 = l2.Id;
        }

        // 3. Admin creates a Collection (Draft)
        var createCmd = new CreateCollectionCommand(
            "Travel English Collection",
            "Learn essential phrases for traveling abroad",
            null,
            "A2",
            1,
            new List<Guid> { lessonId1, lessonId2 });

        var createReq = new HttpRequestMessage(HttpMethod.Post, "/api/admin/collections")
        {
            Content = JsonContent.Create(createCmd)
        };
        createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createRes = await _client.SendAsync(createReq);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);

        var createResult = await createRes.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotNull(createResult);
        var collectionId = createResult.Id;

        // 4. Student queries collections (Draft should NOT be visible)
        var studentReq = new HttpRequestMessage(HttpMethod.Get, "/api/collections");
        studentReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
        var studentRes = await _client.SendAsync(studentReq);
        Assert.Equal(HttpStatusCode.OK, studentRes.StatusCode);
        var studentPaged = await studentRes.Content.ReadFromJsonAsync<PagedResult<CollectionSummaryDto>>();
        Assert.NotNull(studentPaged);
        Assert.DoesNotContain(studentPaged.Items, c => c.Id == collectionId);

        // 5. Admin updates collection to Published
        var updateCmd = new UpdateCollectionCommand(
            collectionId,
            "Travel English Collection (Updated)",
            "Learn essential phrases for traveling abroad - master edition",
            null,
            "A2",
            "Published",
            1,
            new List<Guid> { lessonId2, lessonId1 }); // Reordered

        var updateReq = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/collections/{collectionId}")
        {
            Content = JsonContent.Create(updateCmd)
        };
        updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var updateRes = await _client.SendAsync(updateReq);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        // 6. Student queries published collections again (Should now be visible)
        var studentReq2 = new HttpRequestMessage(HttpMethod.Get, "/api/collections");
        studentReq2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
        var studentRes2 = await _client.SendAsync(studentReq2);
        Assert.Equal(HttpStatusCode.OK, studentRes2.StatusCode);
        var studentPaged2 = await studentRes2.Content.ReadFromJsonAsync<PagedResult<CollectionSummaryDto>>();
        Assert.NotNull(studentPaged2);
        var visibleCol = Assert.Single(studentPaged2.Items, c => c.Id == collectionId);
        Assert.Equal("Travel English Collection (Updated)", visibleCol.Title);
        Assert.Equal(2, visibleCol.TotalLessons);

        // 7. Student retrieves collection detail
        var detailReq = new HttpRequestMessage(HttpMethod.Get, $"/api/collections/{collectionId}");
        detailReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", studentToken);
        var detailRes = await _client.SendAsync(detailReq);
        Assert.Equal(HttpStatusCode.OK, detailRes.StatusCode);
        var detailDto = await detailRes.Content.ReadFromJsonAsync<CollectionDetailDto>();
        Assert.NotNull(detailDto);
        Assert.Equal(2, detailDto.Lessons.Count);
        // Verify suggested order
        Assert.Equal(lessonId2, detailDto.Lessons[0].LessonId);
        Assert.Equal(1, detailDto.Lessons[0].Order);
        Assert.Equal(lessonId1, detailDto.Lessons[1].LessonId);
        Assert.Equal(2, detailDto.Lessons[1].Order);

        // 8. Admin deletes collection -> verify lessons remain intact
        var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/collections/{collectionId}");
        deleteReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var deleteRes = await _client.SendAsync(deleteReq);
        Assert.Equal(HttpStatusCode.OK, deleteRes.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var colInDb = await db.Collections.FindAsync(collectionId);
            Assert.Null(colInDb);

            var l1InDb = await db.Lessons.FindAsync(lessonId1);
            var l2InDb = await db.Lessons.FindAsync(lessonId2);
            Assert.NotNull(l1InDb);
            Assert.NotNull(l2InDb);
        }
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
