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

    // TruncateDayName — abbreviation-preferred truncation

    [Theory]
    [InlineData("воскресенье", "вс", 3, "вс")]
    [InlineData("понедельник", "пн", 3, "пн")]
    [InlineData("вторник", "вт", 3, "вт")]
    [InlineData("среда", "ср", 3, "ср")]
    [InlineData("четверг", "чт", 3, "чт")]
    [InlineData("пятница", "пт", 3, "пт")]
    [InlineData("суббота", "сб", 3, "сб")]
    public void TruncateDayName_RussianWithThreeChars_ReturnsStandardAbbreviation(string fullName, string abbrev, int maxLen, string expected)
    {
        // Arrange & Act
        var result = fullName.TruncateDayName(abbrev, maxLen);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("воскресенье", "вс", 2, "вс")]
    [InlineData("понедельник", "пн", 2, "пн")]
    [InlineData("вторник", "вт", 2, "вт")]
    [InlineData("среда", "ср", 2, "ср")]
    [InlineData("четверг", "чт", 2, "чт")]
    [InlineData("пятница", "пт", 2, "пт")]
    [InlineData("суббота", "сб", 2, "сб")]
    public void TruncateDayName_RussianWithTwoChars_ReturnsStandardAbbreviation(string fullName, string abbrev, int maxLen, string expected)
    {
        // Arrange & Act
        var result = fullName.TruncateDayName(abbrev, maxLen);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("воскресенье", "вс", 1, "в")]
    [InlineData("понедельник", "пн", 1, "п")]
    public void TruncateDayName_WhenAbbreviationExceedsMaxLength_FallsBackToCharacterTruncation(string fullName, string abbrev, int maxLen, string expected)
    {
        // Arrange & Act
        var result = fullName.TruncateDayName(abbrev, maxLen);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Sunday", "Sun", 3, "Sun")]
    [InlineData("Monday", "Mon", 3, "Mon")]
    [InlineData("Tuesday", "Tue", 3, "Tue")]
    [InlineData("Wednesday", "Wed", 3, "Wed")]
    [InlineData("Thursday", "Thu", 3, "Thu")]
    [InlineData("Friday", "Fri", 3, "Fri")]
    [InlineData("Saturday", "Sat", 3, "Sat")]
    public void TruncateDayName_EnglishWithThreeChars_UnchangedBehavior(string fullName, string abbrev, int maxLen, string expected)
    {
        // Arrange & Act
        var result = fullName.TruncateDayName(abbrev, maxLen);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Sunday", "Sun", 2, "Su")]
    [InlineData("Monday", "Mon", 2, "Mo")]
    public void TruncateDayName_WhenAbbreviationDoesNotFit_FallsBackToCharacterTruncation(string fullName, string abbrev, int maxLen, string expected)
    {
        // Arrange & Act
        var result = fullName.TruncateDayName(abbrev, maxLen);

        // Assert
        result.Should().Be(expected);
    }
}