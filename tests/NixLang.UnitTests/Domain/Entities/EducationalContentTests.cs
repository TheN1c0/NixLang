using System;
using NixLang.Domain.Entities;
using NixLang.Domain.Enums;
using NixLang.Domain.ValueObjects;
using Xunit;

namespace NixLang.UnitTests.Domain.Entities;

public class EducationalContentTests
{
    [Fact]
    public void Create_ValidParameters_ShouldInstantiateEducationalContent()
    {
        var title = "False Friends in English";
        var summary = "Common misconceptions for Spanish speakers";
        var body = "# False Friends\nActually != Actualmente";
        var type = EducationalContentType.CommonMistake;
        var level = ReferenceLevel.A2;

        var content = new EducationalContent(title, summary, body, type, level);

        Assert.NotEqual(Guid.Empty, content.Id);
        Assert.Equal(title, content.Title);
        Assert.Equal(summary, content.Summary);
        Assert.Equal(body, content.Body);
        Assert.Equal(type, content.Type);
        Assert.Equal(level, content.ReferenceLevel);
        Assert.Equal(PublicationStatus.Draft, content.Status);
    }

    [Theory]
    [InlineData("", "Body")]
    [InlineData("   ", "Body")]
    [InlineData(null, "Body")]
    [InlineData("Title", "")]
    [InlineData("Title", "   ")]
    [InlineData("Title", null)]
    public void Create_InvalidParameters_ShouldThrowArgumentException(string? title, string? body)
    {
        Assert.Throws<ArgumentException>(() =>
            new EducationalContent(title!, "Summary", body!, EducationalContentType.Explanation));
    }

    [Fact]
    public void Update_ValidParameters_ShouldUpdateProperties()
    {
        var content = new EducationalContent("Initial", "Initial Sum", "Initial Body", EducationalContentType.Concept);

        content.Update("Updated Title", "Updated Sum", "Updated Body", EducationalContentType.Tip, ReferenceLevel.B1);

        Assert.Equal("Updated Title", content.Title);
        Assert.Equal("Updated Sum", content.Summary);
        Assert.Equal("Updated Body", content.Body);
        Assert.Equal(EducationalContentType.Tip, content.Type);
        Assert.Equal(ReferenceLevel.B1, content.ReferenceLevel);
        Assert.NotNull(content.UpdatedAt);
    }

    [Fact]
    public void StatusTransitions_ShouldChangeStatusProperly()
    {
        var content = new EducationalContent("Title", "Summary", "Body", EducationalContentType.Rule);
        Assert.Equal(PublicationStatus.Draft, content.Status);

        content.Publish();
        Assert.Equal(PublicationStatus.Published, content.Status);

        content.Disable();
        Assert.Equal(PublicationStatus.Disabled, content.Status);

        content.SetDraft();
        Assert.Equal(PublicationStatus.Draft, content.Status);
    }

    [Fact]
    public void LessonBlock_CreateContentBlock_ShouldSetTypeAndReferencedId()
    {
        var lessonId = Guid.NewGuid();
        var contentId = Guid.NewGuid();

        var block = LessonBlock.CreateContentBlock(lessonId, 1, contentId);

        Assert.Equal(lessonId, block.LessonId);
        Assert.Equal(LessonBlockType.Content, block.Type);
        Assert.Equal(1, block.Sequence);
        Assert.Equal(contentId, block.ReferencedEducationalContentId);
        Assert.Null(block.ReferencedExerciseId);
    }

    [Fact]
    public void LessonBlock_CreateContentBlock_WithEmptyGuid_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            LessonBlock.CreateContentBlock(Guid.NewGuid(), 1, Guid.Empty));
    }
}
