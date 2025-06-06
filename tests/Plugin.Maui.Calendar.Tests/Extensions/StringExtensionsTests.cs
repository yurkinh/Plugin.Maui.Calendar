using FluentAssertions;
using System.Globalization;
using Plugin.Maui.Calendar.Tests.TestableCode.Extensions;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("hello", "Hello")]
    [InlineData("HELLO", "HELLO")]
    [InlineData("h", "H")]
    [InlineData("hello world", "Hello world")]
    [InlineData("123test", "123test")]
    [InlineData("test123", "Test123")]
    [InlineData("tEST", "TEST")]
    public void Capitalize_ShouldHandleVariousInputs(string input, string expected)
    {
        // Arrange & Act
        var result = input.Capitalize();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Capitalize_ShouldHandleNullString()
    {
        // Arrange
        string? input = null;

        // Act & Assert
        Action act = () => input!.Capitalize();
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Capitalize_ShouldHandleSingleCharacter()
    {
        // Arrange
        var input = "a";

        // Act
        var result = input.Capitalize();

        // Assert
        result.Should().Be("A");
    }

    [Fact]
    public void Capitalize_ShouldNotChangeNumbersOrSymbols()
    {
        // Arrange
        var inputs = new[] { "123", "!@#", "_test", "-hello" };

        // Act & Assert
        foreach (var input in inputs)
        {
            var result = input.Capitalize();
            if (input.Length > 0)
            {
                result[0].Should().Be(char.ToUpperInvariant(input[0]));
                if (input.Length > 1)
                {
                    result.Substring(1).Should().Be(input.Substring(1));
                }
            }
        }
    }
}