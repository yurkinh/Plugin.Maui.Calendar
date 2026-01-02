using FluentAssertions;
using System.Globalization;
using Plugin.Maui.Calendar.Tests.TestableCode.Extensions;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Extensions;

public class ExtensionsTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("hello", "Hello")]
    [InlineData("HELLO", "HELLO")]
    [InlineData("h", "H")]
    [InlineData("hello world", "Hello world")]
    public void Capitalize_ShouldCapitalizeFirstCharacter(string input, string expected)
    {
        // Arrange & Act
        var result = input.Capitalize();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartDayOfMonth_ShouldReturnFirstDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);

        // Act
        var result = date.StartDayOfMonth();

        // Assert
        result.Should().Be(new DateTime(2024, 3, 1));
    }

    [Fact]
    public void EndDayOfMonth_ShouldReturnLastDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 3, 15);

        // Act
        var result = date.EndDayOfMonth();

        // Assert
        result.Should().Be(new DateTime(2024, 3, 31));
    }

    [Fact]
    public void EndDayOfMonth_ShouldHandleFebruaryInLeapYear()
    {
        // Arrange
        var date = new DateTime(2024, 2, 15); // 2024 is a leap year

        // Act
        var result = date.EndDayOfMonth();

        // Assert
        result.Should().Be(new DateTime(2024, 2, 29));
    }

    [Fact]
    public void EndDayOfMonth_ShouldHandleFebruaryInNonLeapYear()
    {
        // Arrange
        var date = new DateTime(2023, 2, 15); // 2023 is not a leap year

        // Act
        var result = date.EndDayOfMonth();

        // Assert
        result.Should().Be(new DateTime(2023, 2, 28));
    }

    [Fact]
    public void DaysInMonth_ShouldReturnCorrectNumberOfDays()
    {
        // Arrange
        var marchDate = new DateTime(2024, 3, 15);
        var februaryLeapDate = new DateTime(2024, 2, 15);
        var februaryNonLeapDate = new DateTime(2023, 2, 15);

        // Act & Assert
        marchDate.DaysInMonth().Should().Be(31);
        februaryLeapDate.DaysInMonth().Should().Be(29);
        februaryNonLeapDate.DaysInMonth().Should().Be(28);
    }

    [Fact]
    public void FirstDayOfMonth_ShouldReturnFirstDayOfMonth()
    {
        // Arrange
        var date = new DateTime(2024, 7, 25);

        // Act
        var result = date.StartDayOfMonth();

        // Assert
        result.Should().Be(new DateTime(2024, 7, 1));
    }

    [Theory]
    [InlineData("2024-01-01", "en-US", 1)] // January 1st, 2024 is a Monday
    [InlineData("2024-01-07", "en-US", 2)] // January 7th, 2024 is a Sunday (second week)
    [InlineData("2024-01-08", "en-US", 2)] // January 8th, 2024 is a Monday (second week)
    [InlineData("2024-01-31", "en-US", 5)] // January 31st, 2024 is a Wednesday (fifth week)
    public void WeekOfMonth_ShouldCalculateCorrectWeek(string dateString, string cultureName, int expectedWeek)
    {
        // Arrange
        var date = DateTime.Parse(dateString);
        var culture = new CultureInfo(cultureName);

        // Act
        var result = date.WeekOfMonth(culture);

        // Assert
        result.Should().Be(expectedWeek);
    }

    [Theory]
    [InlineData("2024-01-01", "en-US", 5)] // January 2024 has 5 weeks
    [InlineData("2024-02-01", "en-US", 5)] // February 2024 has 5 weeks
    [InlineData("2024-03-01", "en-US", 6)] // March 2024 has 6 weeks
    public void WeeksInMonth_ShouldCalculateCorrectNumberOfWeeks(string dateString, string cultureName, int expectedWeeks)
    {
        // Arrange
        var date = DateTime.Parse(dateString);
        var culture = new CultureInfo(cultureName);

        // Act
        var result = date.WeeksInMonth(culture);

        // Assert
        result.Should().Be(expectedWeeks);
    }

    [Fact]
    public void WeekOfMonth_ShouldHandleDifferentCultures()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1); // Monday
        var usCulture = new CultureInfo("en-US"); // Week starts on Sunday
        var germanCulture = new CultureInfo("de-DE"); // Week starts on Monday

        // Act
        var usResult = date.WeekOfMonth(usCulture);
        var germanResult = date.WeekOfMonth(germanCulture);

        // Assert
        // Results might differ based on when the week starts
        usResult.Should().BePositive();
        germanResult.Should().BePositive();
    }
}