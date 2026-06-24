using FluentAssertions;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.ViewLayoutEngines;

/// <summary>
/// Verifies <see cref="ViewLayoutBase.IsWeekendColumn"/>, which decides which grid columns
/// receive the weekend background band. The mapping must follow <c>FirstDayOfWeek</c> so the
/// band lines up with the Saturday/Sunday day-of-week titles in every configuration.
/// </summary>
public class WeekendColumnTests
{
    [Theory]
    // FirstDayOfWeek = Sunday -> columns: Sun Mon Tue Wed Thu Fri Sat
    [InlineData(DayOfWeek.Sunday, 0, true)]   // Sun
    [InlineData(DayOfWeek.Sunday, 1, false)]  // Mon
    [InlineData(DayOfWeek.Sunday, 5, false)]  // Fri
    [InlineData(DayOfWeek.Sunday, 6, true)]   // Sat
    // FirstDayOfWeek = Monday -> columns: Mon Tue Wed Thu Fri Sat Sun
    [InlineData(DayOfWeek.Monday, 0, false)]  // Mon
    [InlineData(DayOfWeek.Monday, 4, false)]  // Fri
    [InlineData(DayOfWeek.Monday, 5, true)]   // Sat
    [InlineData(DayOfWeek.Monday, 6, true)]   // Sun
    // FirstDayOfWeek = Saturday -> columns: Sat Sun Mon Tue Wed Thu Fri
    [InlineData(DayOfWeek.Saturday, 0, true)]  // Sat
    [InlineData(DayOfWeek.Saturday, 1, true)]  // Sun
    [InlineData(DayOfWeek.Saturday, 2, false)] // Mon
    [InlineData(DayOfWeek.Saturday, 6, false)] // Fri
    public void IsWeekendColumn_MapsColumnsRelativeToFirstDayOfWeek(
        DayOfWeek firstDayOfWeek, int column, bool expected)
    {
        ViewLayoutBase.IsWeekendColumn(firstDayOfWeek, column)
            .Should().Be(expected);
    }

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Saturday)]
    public void IsWeekendColumn_AlwaysMarksExactlyTwoWeekendColumnsPerWeek(DayOfWeek firstDayOfWeek)
    {
        var weekendColumns = Enumerable.Range(0, 7)
            .Count(col => ViewLayoutBase.IsWeekendColumn(firstDayOfWeek, col));

        weekendColumns.Should().Be(2,
            "every week has exactly one Saturday and one Sunday regardless of FirstDayOfWeek");
    }
}
