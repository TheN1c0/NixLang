using NixLang.Domain.ValueObjects;

namespace NixLang.UnitTests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("some_email@subdomain.example.org")]
    public void Create_WithValidEmail_ShouldCreateInstance(string value)
    {
        // Act
        var email = Email.Create(value);

        // Assert
        Assert.NotNull(email);
        Assert.Equal(value.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullEmail_ShouldThrowArgumentException(string? value)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(value!));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test@example")]
    [InlineData("test @example.com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowArgumentException(string value)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(value));
    }

    [Fact]
    public void Equals_WithSameEmailDifferentCasing_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("TEST@example.com");
        var email2 = Email.Create("test@EXAMPLE.COM");

        // Act & Assert
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
        Assert.True(email1.Equals(email2));
    }

    [Fact]
    public void Equals_WithDifferentEmails_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
        Assert.True(email1 != email2);
        Assert.False(email1.Equals(email2));
    }
}
