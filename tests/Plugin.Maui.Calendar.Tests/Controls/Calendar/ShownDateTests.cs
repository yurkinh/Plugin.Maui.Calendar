using FluentAssertions;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies <c>ShownDate</c> and its <c>Day</c>, <c>Month</c> and <c>Year</c> parts, which follow
/// each other both ways, and the days shown at the ends of the range of <see cref="DateTime"/>.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class ShownDateTests
{
    // ── Day, Month and Year ──────────────────────────────────────────────────

    [Fact]
    public void Defaults_AreTodayWhenTheCalendarIsCreated()
    {
        var today = DateTime.Today;
        var calendar = new TestCalendar();

        // A run that crosses midnight between the two reads is accepted.
        calendar.ShownDate.Should().BeOneOf(today, DateTime.Today);
        calendar.Day.Should().Be(calendar.ShownDate.Day);
        calendar.Month.Should().Be(calendar.ShownDate.Month);
        calendar.Year.Should().Be(calendar.ShownDate.Year);
    }

    [Fact]
    public void ShownDate_UpdatesDayMonthAndYear()
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2025, 5, 15) };

        calendar.ShownDate = new DateTime(2026, 11, 3);

        calendar.Day.Should().Be(3);
        calendar.Month.Should().Be(11);
        calendar.Year.Should().Be(2026);
    }

    [Fact]
    public void DayMonthAndYear_MoveShownDate()
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2025, 5, 15) };

        calendar.Day = 20;
        calendar.ShownDate.Should().Be(new DateTime(2025, 5, 20));

        calendar.Month = 7;
        calendar.ShownDate.Should().Be(new DateTime(2025, 7, 20));

        calendar.Year = 2027;
        calendar.ShownDate.Should().Be(new DateTime(2027, 7, 20));
        calendar.LayoutUnitText.Should().Be("July");
    }

    [Fact]
    public void Day_LaterThanTheLastDayOfTheMonth_MovesToTheLastDay()
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2025, 2, 10) };

        calendar.Day = 31;

        calendar.ShownDate.Should().Be(new DateTime(2025, 2, 28));
        calendar.Day.Should().Be(28);
    }

    [Fact]
    public void Month_WithFewerDays_MovesToItsLastDay()
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2025, 3, 31) };

        calendar.Month = 4;

        calendar.ShownDate.Should().Be(new DateTime(2025, 4, 30));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Month_OutsideOneToTwelve_Throws(int month)
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2025, 5, 15) };

        var setMonth = () => calendar.Month = month;

        setMonth.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Year_FromFebruary29ToAYearThatIsNotALeapYear_MovesToFebruary28()
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2024, 2, 29) };

        calendar.Year = 2025;

        calendar.ShownDate.Should().Be(new DateTime(2025, 2, 28));
    }

    // ── The ends of the range of DateTime ────────────────────────────────────

    [Fact]
    public void ShownDate_DefaultDateTime_ShowsJanuaryOfYearOneWithTheCellsBeforeItEmpty()
    {
        // default(DateTime) is what a binding to an unset DateTime property gives. January 1 of
        // year 1 is a Monday, so with Sunday first the first cell would be a day before DateTime.MinValue.
        var calendar = new TestCalendar { ShownDate = default };

        var days = calendar.Days();

        days[0].Day.Should().BeEmpty();
        days[0].IsVisible.Should().BeFalse();
        days[1].Date.Should().Be(DateTime.MinValue);
        days[1].Day.Should().Be("1");
        days[1].IsThisMonth.Should().BeTrue();
        days[41].Date.Should().Be(DateTime.MinValue.AddDays(40));
        calendar.VisibleStartDate.Should().Be(DateTime.MinValue);
        calendar.VisibleEndDate.Should().Be(DateTime.MinValue.AddDays(40));
        calendar.LayoutUnitText.Should().Be("January");
    }

    [Fact]
    public void ShownDate_FirstWeekOfDateTimeInTheWeekLayout_LeavesTheCellsBeforeItEmpty()
    {
        var calendar = new TestCalendar { CalendarLayout = WeekLayout.Week, ShownDate = new DateTime(1, 1, 3) };

        var days = calendar.Days();

        days[0].Day.Should().BeEmpty();
        days.Skip(1).Select(day => day.Date).Should().Equal(Enumerable.Range(0, 6).Select(day => DateTime.MinValue.AddDays(day)));
        calendar.VisibleEndDate.Should().Be(DateTime.MinValue.AddDays(5));
    }

    [Fact]
    public void ShownDate_LastMonthOfDateTime_LeavesTheCellsAfterItEmpty()
    {
        // December 9999 starts on a Wednesday; with Sunday first the grid runs past December 31.
        var calendar = new TestCalendar { ShownDate = DateTime.MaxValue };

        var days = calendar.Days();

        days.Where(day => day.Day.Length > 0).Select(day => day.Date).Last().Should().Be(DateTime.MaxValue.Date);
        days.Last().Day.Should().BeEmpty();
        days.Last().IsDisabled.Should().BeTrue();
        calendar.VisibleEndDate.Should().Be(DateTime.MaxValue.Date);
    }
}
