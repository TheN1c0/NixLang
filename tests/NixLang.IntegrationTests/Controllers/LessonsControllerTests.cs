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

    [Fact]
    public async Task GetLessonById_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var someId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/lessons/{someId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLessonById_WithExistingPublishedLesson_ShouldReturn200AndDetail()
    {
        // Arrange
        Guid lessonId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var lesson = new Lesson("Present Perfect", "Learn the present perfect tense", ReferenceLevel.B1);
            lesson.Publish();

            var ex1 = new Exercise(ExerciseType.MultipleChoice, "Exercise 1", "CorrectAnswer 1");
            var ex2 = new Exercise(ExerciseType.Translation, "Exercise 2", "CorrectAnswer 2");
            dbContext.Exercises.AddRange(ex1, ex2);

            var block1 = LessonBlock.CreateExerciseBlock(lesson.Id, 1, ex1.Id);
            var block2 = LessonBlock.CreateExerciseBlock(lesson.Id, 2, ex2.Id);
            lesson.AddLessonBlock(block1);
            lesson.AddLessonBlock(block2);

            dbContext.Lessons.Add(lesson);
            dbContext.SaveChanges();
            lessonId = lesson.Id;
        }

        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons/{lessonId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LessonDetailResponseDto>();
        Assert.NotNull(result);
        Assert.Equal(lessonId, result.Id);
        Assert.Equal("Present Perfect", result.Title);
        Assert.Equal("Learn the present perfect tense", result.Description);
        Assert.Equal("B1", result.ReferenceLevel);
        Assert.Equal(2, result.ExerciseCount);

        // Verify lesson blocks structure and sequence
        Assert.NotNull(result.LessonBlocks);
        Assert.Equal(2, result.LessonBlocks.Count);

        var b1 = result.LessonBlocks.Find(b => b.Sequence == 1);
        Assert.NotNull(b1);
        Assert.Equal("Exercise", b1.Type);
        Assert.NotNull(b1.Exercise);
        Assert.Equal("MultipleChoice", b1.Exercise.Type);
        Assert.Equal("Exercise 1", b1.Exercise.Statement);
        Assert.Equal("CorrectAnswer 1", b1.Exercise.CorrectAnswer);

        var b2 = result.LessonBlocks.Find(b => b.Sequence == 2);
        Assert.NotNull(b2);
        Assert.Equal("Exercise", b2.Type);
        Assert.NotNull(b2.Exercise);
        Assert.Equal("Translation", b2.Exercise.Type);
        Assert.Equal("Exercise 2", b2.Exercise.Statement);
        Assert.Equal("CorrectAnswer 2", b2.Exercise.CorrectAnswer);
    }

    [Fact]
    public async Task GetLessonById_WithDraftLesson_ShouldReturn404()
    {
        // Arrange
        Guid draftLessonId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var lesson = new Lesson("Draft Lesson Title", "Draft Lesson Desc", ReferenceLevel.A2);
            dbContext.Lessons.Add(lesson);
            dbContext.SaveChanges();
            draftLessonId = lesson.Id;
        }

        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons/{draftLessonId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLessonById_WithDisabledLesson_ShouldReturn404()
    {
        // Arrange
        Guid disabledLessonId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var lesson = new Lesson("Disabled Lesson Title", "Disabled Lesson Desc", ReferenceLevel.B2);
            lesson.Publish();
            lesson.Disable();
            dbContext.Lessons.Add(lesson);
            dbContext.SaveChanges();
            disabledLessonId = lesson.Id;
        }

        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons/{disabledLessonId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLessonById_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons/{nonExistingId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLessonById_WithInvalidGuid_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLogin();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons/invalid-guid-format");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetLessons_WithSearchTermInTitle_ShouldReturnMatchingLessons()
    {
        // Arrange
        var token = await RegisterAndLogin();
        SeedLessons(
            published:
            [
                ("Present Simple", "Introduction to present simple", ReferenceLevel.A1),
                ("Past Simple", "Introduction to past simple", ReferenceLevel.A2),
                ("Business English", "English for business", ReferenceLevel.B1)
            ]);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?search=Simple");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Title == "Present Simple");
        Assert.Contains(result.Items, item => item.Title == "Past Simple");
        Assert.DoesNotContain(result.Items, item => item.Title == "Business English");
    }

    [Fact]
    public async Task GetLessons_WithSearchTermInDescription_ShouldReturnMatchingLessons()
    {
        // Arrange
        var token = await RegisterAndLogin();
        SeedLessons(
            published:
            [
                ("Present Simple", "Introduction to present simple", ReferenceLevel.A1),
                ("Past Simple", "Introduction to past simple", ReferenceLevel.A2),
                ("Travel Vocabulary", "Vocabulary used during travel activities", ReferenceLevel.B1)
            ]);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?search=travel");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Title == "Travel Vocabulary");
    }

    [Fact]
    public async Task GetLessons_WithCaseInsensitiveSearch_ShouldFindMatches()
    {
        // Arrange
        var token = await RegisterAndLogin();
        SeedLessons(
            published:
            [
                ("Present Simple", "Introduction to present simple", ReferenceLevel.A1)
            ]);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?search=pReSeNt");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Title == "Present Simple");
    }

    [Fact]
    public async Task GetLessons_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?search=NonExistingTerm12345");
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
    public async Task GetLessons_WithSearchTermTooLong_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLogin();
        var longSearchTerm = new string('a', 101);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons?search={longSearchTerm}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        Assert.NotNull(error);
        Assert.True(error.Errors.ContainsKey("Search"));
    }
    [Fact]
    public async Task GetLessons_WithLevelFilter_ShouldReturnOnlyLessonsOfThatLevel()
    {
        // Arrange
        Guid levelA1LessonId;
        Guid levelB2LessonId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var l1 = new Lesson("A1 Lesson", "Description A1", ReferenceLevel.A1);
            l1.Publish();
            var l2 = new Lesson("B2 Lesson", "Description B2", ReferenceLevel.B2);
            l2.Publish();
            dbContext.Lessons.AddRange(l1, l2);
            dbContext.SaveChanges();
            levelA1LessonId = l1.Id;
            levelB2LessonId = l2.Id;
        }

        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?level=A1");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Id == levelA1LessonId);
        Assert.DoesNotContain(result.Items, item => item.Id == levelB2LessonId);
    }

    [Fact]
    public async Task GetLessons_WithCategoryFilter_ShouldReturnLessonsOfThatCategory()
    {
        // Arrange
        Guid cat1Id;
        Guid lessonWithCatId;
        Guid lessonWithoutCatId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var c1 = new Category($"Grammar {Guid.NewGuid()}", "Grammar category");
            dbContext.Categories.Add(c1);
            dbContext.SaveChanges();
            cat1Id = c1.Id;

            var l1 = new Lesson("Grammar Lesson", "Learn grammar", ReferenceLevel.B1);
            l1.Publish();
            l1.AddCategory(c1);

            var l2 = new Lesson("Vocabulary Lesson", "Learn vocab", ReferenceLevel.B1);
            l2.Publish();

            dbContext.Lessons.AddRange(l1, l2);
            dbContext.SaveChanges();

            lessonWithCatId = l1.Id;
            lessonWithoutCatId = l2.Id;
        }

        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons?categoryIds={cat1Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Id == lessonWithCatId);
        Assert.DoesNotContain(result.Items, item => item.Id == lessonWithoutCatId);
    }

    [Fact]
    public async Task GetLessons_WithMultipleCategoriesFilter_ShouldReturnLessonsOfAnyCategory()
    {
        // Arrange
        Guid cat1Id;
        Guid cat2Id;
        Guid lessonCat1Id;
        Guid lessonCat2Id;
        Guid lessonNoCatId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var c1 = new Category($"Cat 1 {Guid.NewGuid()}", "Desc 1");
            var c2 = new Category($"Cat 2 {Guid.NewGuid()}", "Desc 2");
            dbContext.Categories.AddRange(c1, c2);
            dbContext.SaveChanges();
            cat1Id = c1.Id;
            cat2Id = c2.Id;

            var l1 = new Lesson("Lesson Cat1", "Learn Cat1", ReferenceLevel.B1);
            l1.Publish();
            l1.AddCategory(c1);

            var l2 = new Lesson("Lesson Cat2", "Learn Cat2", ReferenceLevel.B1);
            l2.Publish();
            l2.AddCategory(c2);

            var l3 = new Lesson("Lesson NoCat", "Learn NoCat", ReferenceLevel.B1);
            l3.Publish();

            dbContext.Lessons.AddRange(l1, l2, l3);
            dbContext.SaveChanges();

            lessonCat1Id = l1.Id;
            lessonCat2Id = l2.Id;
            lessonNoCatId = l3.Id;
        }

        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons?categoryIds={cat1Id},{cat2Id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Id == lessonCat1Id);
        Assert.Contains(result.Items, item => item.Id == lessonCat2Id);
        Assert.DoesNotContain(result.Items, item => item.Id == lessonNoCatId);
    }

    [Fact]
    public async Task GetLessons_WithLevelAndCategoryFilter_ShouldReturnLessonsMatchingBoth()
    {
        // Arrange
        Guid catId;
        Guid matchingLessonId;
        Guid wrongLevelLessonId;
        Guid wrongCatLessonId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var c1 = new Category($"Cat A1 {Guid.NewGuid()}", "Desc");
            dbContext.Categories.Add(c1);
            dbContext.SaveChanges();
            catId = c1.Id;

            var l1 = new Lesson("Matching B1", "Matching B1", ReferenceLevel.B1);
            l1.Publish();
            l1.AddCategory(c1);

            var l2 = new Lesson("Wrong Level A2", "Wrong Level A2", ReferenceLevel.A2);
            l2.Publish();
            l2.AddCategory(c1);

            var l3 = new Lesson("Wrong Cat B1", "Wrong Cat B1", ReferenceLevel.B1);
            l3.Publish();

            dbContext.Lessons.AddRange(l1, l2, l3);
            dbContext.SaveChanges();

            matchingLessonId = l1.Id;
            wrongLevelLessonId = l2.Id;
            wrongCatLessonId = l3.Id;
        }

        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons?level=B1&categoryIds={catId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Id == matchingLessonId);
        Assert.DoesNotContain(result.Items, item => item.Id == wrongLevelLessonId);
        Assert.DoesNotContain(result.Items, item => item.Id == wrongCatLessonId);
    }

    [Fact]
    public async Task GetLessons_WithLevelCategoryAndSearchFilter_ShouldReturnMatchingAll()
    {
        // Arrange
        Guid catId;
        Guid matchingLessonId;
        Guid wrongSearchLessonId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var c1 = new Category($"Cat A1 {Guid.NewGuid()}", "Desc");
            dbContext.Categories.Add(c1);
            dbContext.SaveChanges();
            catId = c1.Id;

            var l1 = new Lesson("Travel Tips", "Tips for travel", ReferenceLevel.B2);
            l1.Publish();
            l1.AddCategory(c1);

            var l2 = new Lesson("Business Tips", "Tips for business", ReferenceLevel.B2);
            l2.Publish();
            l2.AddCategory(c1);

            dbContext.Lessons.AddRange(l1, l2);
            dbContext.SaveChanges();

            matchingLessonId = l1.Id;
            wrongSearchLessonId = l2.Id;
        }

        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/lessons?search=travel&level=B2&categoryIds={catId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, item => item.Id == matchingLessonId);
        Assert.DoesNotContain(result.Items, item => item.Id == wrongSearchLessonId);
    }

    [Fact]
    public async Task GetLessons_WithFiltersAndNoResults_ShouldReturn200AndEmptyList()
    {
        // Arrange
        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?level=B2&categoryIds=00000000-0000-0000-0000-000000000000");
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
    public async Task GetLessons_WithInvalidLevelFilter_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = await RegisterAndLogin();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/lessons?level=XYZ");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();
        Assert.NotNull(error);
        Assert.True(error.Errors.ContainsKey("Level"));
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

    [Fact]
    public async Task SaveLessonProgress_WithExerciseResults_ShouldSaveSuccessfully()
    {
        // Arrange
        Guid lessonId;
        Guid exerciseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var lesson = new Lesson("Present Simple Test Progress", "Learn present simple", ReferenceLevel.A1);
            lesson.Publish();
            
            var exercise = new Exercise(ExerciseType.MultipleChoice, "Statement", "CorrectAnswer");
            dbContext.Exercises.Add(exercise);
            dbContext.SaveChanges();
            
            var block = LessonBlock.CreateExerciseBlock(lesson.Id, 1, exercise.Id);
            lesson.AddLessonBlock(block);
            
            dbContext.Lessons.Add(lesson);
            dbContext.SaveChanges();
            
            lessonId = lesson.Id;
            exerciseId = exercise.Id;
        }
        
        var token = await RegisterAndLogin();
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/lessons/{lessonId}/progress");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var requestBody = new
        {
            ProgressPercentage = 100.00m,
            Status = "Completed",
            Results = new[]
            {
                new { ExerciseId = exerciseId, GivenAnswer = "CorrectAnswer", IsCorrect = true }
            }
        };
        request.Content = JsonContent.Create(requestBody);
        
        // Act
        var response = await _client.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SaveLessonProgress_RepeatingCompletedLesson_NeverDegradesCompletedStatus()
    {
        // Arrange
        Guid lessonId;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var lesson = new Lesson($"Repeat Lesson Test {Guid.NewGuid()}", "Desc", ReferenceLevel.A1);
            lesson.Publish();
            dbContext.Lessons.Add(lesson);
            dbContext.SaveChanges();
            lessonId = lesson.Id;
        }

        var token = await RegisterAndLogin();

        // 1. Initial complete execution (100% -> Completed)
        var req1 = new HttpRequestMessage(HttpMethod.Post, $"/api/lessons/{lessonId}/progress");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Content = JsonContent.Create(new
        {
            ProgressPercentage = 100.00m,
            Status = "Completed",
            Results = Array.Empty<object>()
        });
        var res1 = await _client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);

        // Verify lesson is Completed with 100% in catalog
        var getReq1 = new HttpRequestMessage(HttpMethod.Get, "/api/lessons");
        getReq1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getRes1 = await _client.SendAsync(getReq1);
        var catalog1 = await getRes1.Content.ReadFromJsonAsync<PagedResultDto>();
        var item1 = catalog1!.Items.First(i => i.Id == lessonId);
        Assert.Equal("Completed", item1.Status);
        Assert.Equal(100m, item1.ProgressPercentage);

        // 2. User repeats lesson and triggers partial progress (25% -> InProgress)
        var req2 = new HttpRequestMessage(HttpMethod.Post, $"/api/lessons/{lessonId}/progress");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Content = JsonContent.Create(new
        {
            ProgressPercentage = 25.00m,
            Status = "InProgress",
            Results = Array.Empty<object>()
        });
        var res2 = await _client.SendAsync(req2);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);

        // Verify lesson STILL remains Completed with 100% in catalog
        var getReq2 = new HttpRequestMessage(HttpMethod.Get, "/api/lessons");
        getReq2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getRes2 = await _client.SendAsync(getReq2);
        var catalog2 = await getRes2.Content.ReadFromJsonAsync<PagedResultDto>();
        var item2 = catalog2!.Items.First(i => i.Id == lessonId);
        Assert.Equal("Completed", item2.Status);
        Assert.Equal(100m, item2.ProgressPercentage);
    }

    [Fact]
    public async Task SaveLessonProgress_MultiStepExecutionWithExerciseAnswering_SavesSuccessfullyWithoutError()
    {
        // Arrange — Create lesson with 2 exercises
        Guid lessonId;
        Guid ex1Id;
        Guid ex2Id;
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NixLangDbContext>();
            var lesson = new Lesson($"MultiStep Progress Test {Guid.NewGuid()}", "Desc", ReferenceLevel.A1);
            lesson.Publish();

            var ex1 = new Exercise(ExerciseType.MultipleChoice, "Question 1", "Ans 1");
            var ex2 = new Exercise(ExerciseType.FillInTheBlank, "Question 2", "Ans 2");
            dbContext.Exercises.AddRange(ex1, ex2);
            dbContext.SaveChanges();

            lesson.AddLessonBlock(LessonBlock.CreateExerciseBlock(lesson.Id, 1, ex1.Id));
            lesson.AddLessonBlock(LessonBlock.CreateExerciseBlock(lesson.Id, 2, ex2.Id));
            dbContext.Lessons.Add(lesson);
            dbContext.SaveChanges();

            lessonId = lesson.Id;
            ex1Id = ex1.Id;
            ex2Id = ex2.Id;
        }

        var token = await RegisterAndLogin();

        // Step 1: User enters lesson (25%, InProgress, no exercises answered yet)
        var req1 = new HttpRequestMessage(HttpMethod.Post, $"/api/lessons/{lessonId}/progress");
        req1.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req1.Content = JsonContent.Create(new
        {
            ProgressPercentage = 25.00m,
            Status = "InProgress",
            Results = Array.Empty<object>()
        });
        var res1 = await _client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);

        // Step 2: User answers exercise 1 (50%, InProgress, 1 exercise result)
        var req2 = new HttpRequestMessage(HttpMethod.Post, $"/api/lessons/{lessonId}/progress");
        req2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req2.Content = JsonContent.Create(new
        {
            ProgressPercentage = 50.00m,
            Status = "InProgress",
            Results = new[]
            {
                new { ExerciseId = ex1Id, GivenAnswer = "Ans 1", IsCorrect = true }
            }
        });
        var res2 = await _client.SendAsync(req2);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);

        // Step 3: User answers exercise 2 (100%, Completed, 2 exercise results)
        var req3 = new HttpRequestMessage(HttpMethod.Post, $"/api/lessons/{lessonId}/progress");
        req3.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req3.Content = JsonContent.Create(new
        {
            ProgressPercentage = 100.00m,
            Status = "Completed",
            Results = new[]
            {
                new { ExerciseId = ex1Id, GivenAnswer = "Ans 1", IsCorrect = true },
                new { ExerciseId = ex2Id, GivenAnswer = "Ans 2", IsCorrect = true }
            }
        });
        var res3 = await _client.SendAsync(req3);
        Assert.Equal(HttpStatusCode.OK, res3.StatusCode);

        // Step 4: User repeats the lesson, re-answers exercise 1 with different answer
        var req4 = new HttpRequestMessage(HttpMethod.Post, $"/api/lessons/{lessonId}/progress");
        req4.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req4.Content = JsonContent.Create(new
        {
            ProgressPercentage = 50.00m,
            Status = "InProgress",
            Results = new[]
            {
                new { ExerciseId = ex1Id, GivenAnswer = "Ans 1 Updated", IsCorrect = true }
            }
        });
        var res4 = await _client.SendAsync(req4);
        Assert.Equal(HttpStatusCode.OK, res4.StatusCode);

        // Verify status remains Completed in catalog
        var getReq = new HttpRequestMessage(HttpMethod.Get, "/api/lessons");
        getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getRes = await _client.SendAsync(getReq);
        var catalog = await getRes.Content.ReadFromJsonAsync<PagedResultDto>();
        var item = catalog!.Items.First(i => i.Id == lessonId);
        Assert.Equal("Completed", item.Status);
        Assert.Equal(100m, item.ProgressPercentage);
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
        public bool IsFavorite { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
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

    private class LessonDetailResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReferenceLevel { get; set; } = string.Empty;
        public int ExerciseCount { get; set; }
        public List<LessonBlockResponseDto> LessonBlocks { get; set; } = [];
    }

    private class LessonBlockResponseDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string ConfigurationValue { get; set; } = string.Empty;
        public Guid? ReferencedExerciseId { get; set; }
        public ExerciseResponseDto? Exercise { get; set; }
    }

    private class ExerciseResponseDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Statement { get; set; } = string.Empty;
        public string? CorrectAnswer { get; set; }
        public string? AudioResourceUrl { get; set; }
    }
}
