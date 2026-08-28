using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using Xunit;

namespace NixLang.UnitTests.Domain.Entities;

public class CollectionTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldCreateCollectionInDraftState()
    {
        // Arrange
        var title = "Inglés para viajar";
        var description = "Prepárate para interactuar en aeropuertos, hoteles y restaurantes.";
        var suggestedLevel = ReferenceLevel.A2;
        var displayOrder = 1;

        // Act
        var collection = new Collection(title, description, null, suggestedLevel, displayOrder);

        // Assert
        Assert.NotEqual(Guid.Empty, collection.Id);
        Assert.Equal(title, collection.Title);
        Assert.Equal(description, collection.Description);
        Assert.Equal(suggestedLevel, collection.SuggestedLevel);
        Assert.Equal(PublicationStatus.Draft, collection.Status);
        Assert.Equal(displayOrder, collection.DisplayOrder);
        Assert.Empty(collection.CollectionLessons);
    }

    [Theory]
    [InlineData("", "Valid description")]
    [InlineData("   ", "Valid description")]
    [InlineData(null, "Valid description")]
    [InlineData("Valid title", "")]
    [InlineData("Valid title", "   ")]
    [InlineData("Valid title", null)]
    public void Constructor_WithInvalidArguments_ShouldThrowArgumentException(string? title, string? description)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Collection(title!, description!));
    }

    [Fact]
    public void AddLesson_ShouldAddLessonWithAutoIncrementedOrder()
    {
        // Arrange
        var collection = new Collection("Title", "Description");
        var lessonId1 = Guid.NewGuid();
        var lessonId2 = Guid.NewGuid();

        // Act
        collection.AddLesson(lessonId1);
        collection.AddLesson(lessonId2);

        // Assert
        Assert.Equal(2, collection.CollectionLessons.Count);
        var lessons = collection.CollectionLessons.ToList();
        Assert.Equal(lessonId1, lessons[0].LessonId);
        Assert.Equal(1, lessons[0].Order);
        Assert.Equal(lessonId2, lessons[1].LessonId);
        Assert.Equal(2, lessons[1].Order);
    }

    [Fact]
    public void AddLesson_WhenLessonAlreadyPresent_ShouldNotDuplicate()
    {
        // Arrange
        var collection = new Collection("Title", "Description");
        var lessonId = Guid.NewGuid();

        // Act
        collection.AddLesson(lessonId);
        collection.AddLesson(lessonId);

        // Assert
        Assert.Single(collection.CollectionLessons);
    }

    [Fact]
    public void RemoveLesson_ShouldRemoveAndReorderRemainingLessons()
    {
        // Arrange
        var collection = new Collection("Title", "Description");
        var lesson1 = Guid.NewGuid();
        var lesson2 = Guid.NewGuid();
        var lesson3 = Guid.NewGuid();

        collection.AddLesson(lesson1);
        collection.AddLesson(lesson2);
        collection.AddLesson(lesson3);

        // Act - remove middle lesson
        collection.RemoveLesson(lesson2);

        // Assert
        Assert.Equal(2, collection.CollectionLessons.Count);
        var lessons = collection.CollectionLessons.ToList();
        Assert.Equal(lesson1, lessons[0].LessonId);
        Assert.Equal(1, lessons[0].Order);
        Assert.Equal(lesson3, lessons[1].LessonId);
        Assert.Equal(2, lessons[1].Order);
    }

    [Fact]
    public void ReorderLessons_WithValidIds_ShouldUpdateOrder()
    {
        // Arrange
        var collection = new Collection("Title", "Description");
        var lesson1 = Guid.NewGuid();
        var lesson2 = Guid.NewGuid();
        var lesson3 = Guid.NewGuid();

        collection.AddLesson(lesson1);
        collection.AddLesson(lesson2);
        collection.AddLesson(lesson3);

        // Act - reverse order
        collection.ReorderLessons(new List<Guid> { lesson3, lesson1, lesson2 });

        // Assert
        var lessons = collection.CollectionLessons.ToList();
        Assert.Equal(lesson3, lessons[0].LessonId);
        Assert.Equal(1, lessons[0].Order);
        Assert.Equal(lesson1, lessons[1].LessonId);
        Assert.Equal(2, lessons[1].Order);
        Assert.Equal(lesson2, lessons[2].LessonId);
        Assert.Equal(3, lessons[2].Order);
    }

    [Fact]
    public void StatusTransitions_ShouldUpdateStatusProperly()
    {
        // Arrange
        var collection = new Collection("Title", "Description");
        Assert.Equal(PublicationStatus.Draft, collection.Status);

        // Act & Assert Publish
        collection.Publish();
        Assert.Equal(PublicationStatus.Published, collection.Status);

        // Act & Assert Disable
        collection.Disable();
        Assert.Equal(PublicationStatus.Disabled, collection.Status);

        // Act & Assert Draft
        collection.SetDraft();
        Assert.Equal(PublicationStatus.Draft, collection.Status);
    }
}
