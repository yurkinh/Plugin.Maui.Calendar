using FluentAssertions;
using System.Globalization;
using Plugin.Maui.Calendar.Tests.TestableCode.Extensions;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Extensions;

public class DateTimeAdvancedExtensionsTests
{
    [Theory]
    [InlineData(2024, 1, 2024, 1, 31)] // January
    [InlineData(2024, 2, 2024, 2, 29)] // Leap year February
    [InlineData(2023, 2, 2023, 2, 28)] // Non-leap year February
    [InlineData(2024, 4, 2024, 4, 30)] // April
    [InlineData(2024, 12, 2024, 12, 31)] // December
    public void StartAndEndDayOfMonth_ShouldWorkCorrectly(int year, int month, int expectedEndYear, int expectedEndMonth, int expectedEndDay)
    {
        // Arrange
        var date = new DateTime(year, month, 15); // Middle of month

        // Act
        var startDay = date.StartDayOfMonth();
        var endDay = date.EndDayOfMonth();

        // Assert
        startDay.Should().Be(new DateTime(year, month, 1));
        endDay.Should().Be(new DateTime(expectedEndYear, expectedEndMonth, expectedEndDay));
    }

    [Theory]
    [InlineData("2024-01-01", "en-US")] // New Year's Day
    [InlineData("2024-02-29", "en-US")] // Leap day
    [InlineData("2024-12-31", "en-US")] // New Year's Eve
    public void FirstDayOfMonth_ShouldAlwaysBeFirstDay(string dateString, string cultureName)
    {
        // Arrange
        var date = DateTime.Parse(dateString);
        var culture = new CultureInfo(cultureName);

        // Act
        var firstDay = date.StartDayOfMonth();

        // Assert
        firstDay.Day.Should().Be(1);
        firstDay.Month.Should().Be(date.Month);
        firstDay.Year.Should().Be(date.Year);
    }

    [Theory]
    [InlineData(2024, 1, 31)] // January
    [InlineData(2024, 4, 30)] // April
    [InlineData(2024, 2, 29)] // February leap year
    [InlineData(2023, 2, 28)] // February non-leap year
    public void DaysInMonth_ShouldReturnCorrectDaysForVariousMonths(int year, int month, int expectedDays)
    {
        // Arrange
        var date = new DateTime(year, month, 1);

        // Act
        var days = date.DaysInMonth();

        // Assert
        days.Should().Be(expectedDays);
    }

    [Theory]
    [InlineData("2024-01-01", "2024-01-31")] // January
    [InlineData("2024-02-01", "2024-02-29")] // February leap year
    [InlineData("2023-02-01", "2023-02-28")] // February non-leap year
    [InlineData("2024-04-01", "2024-04-30")] // April
    [InlineData("2024-12-01", "2024-12-31")] // December
    public void EndDayOfMonth_ShouldCalculateCorrectEndDate(string startDateString, string expectedEndDateString)
    {
        // Arrange
        var startDate = DateTime.Parse(startDateString);
        var expectedEndDate = DateTime.Parse(expectedEndDateString);

        // Act
        var endDate = startDate.EndDayOfMonth();

        // Assert
        endDate.Should().Be(expectedEndDate);
    }

    [Fact]
    public void WeekOfMonth_ShouldHandleVariousCultures()
    {
        // Arrange
        var testDate = new DateTime(2024, 1, 15); // Middle of January
        var cultures = new[] { "en-US", "de-DE", "fr-FR", "ja-JP" };

        // Act & Assert
        foreach (var cultureName in cultures)
        {
            var culture = new CultureInfo(cultureName);
            var weekOfMonth = testDate.WeekOfMonth(culture);

            // Week of month should always be between 1 and 6
            weekOfMonth.Should().BeInRange(1, 6);
        }
    }

    [Fact]
    public void WeeksInMonth_ShouldHandleVariousMonths()
    {
        // Arrange
        var culture = new CultureInfo("en-US");
        var testDates = new[]
        {
            new DateTime(2024, 1, 1), // January
            new DateTime(2024, 2, 1), // February
            new DateTime(2024, 3, 1), // March
            new DateTime(2024, 4, 1), // April
            new DateTime(2024, 5, 1), // May
            new DateTime(2024, 6, 1), // June
        };

        // Act & Assert
        foreach (var date in testDates)
        {
            var weeksInMonth = date.WeeksInMonth(culture);

            // Weeks in month should typically be between 4 and 6
            weeksInMonth.Should().BeInRange(4, 6);
        }
    }
}