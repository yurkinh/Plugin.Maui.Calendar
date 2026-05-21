using FluentAssertions;
using Plugin.Maui.Calendar.Controls;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

public class IsDateDisabledTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    static DateTime Midnight(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0);

    static DateTime NonMidnight(int year, int month, int day) =>
        new(year, month, day, 13, 20, 0);

    // ── boundary: non-midnight MinimumDate ───────────────────────────────────

    [Fact]
    public void IsDateDisabled_BoundaryDayMatchesNonMidnightMinimumDate_ReturnsFalse()
    {
        // The boundary calendar day must NOT be disabled even when MinimumDate
        // carries a non-midnight time (e.g. DateTime.Now.AddDays(-7)).
        var boundaryDay = Midnight(2025, 5, 14);
        var minimumDate = NonMidnight(2025, 5, 14); // same calendar day, non-midnight

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            boundaryDay,
            minimumDate,
            DateTime.MaxValue,
            disabledSet: null);

        result.Should().BeFalse("the boundary day itself should be enabled");
    }

    // ── boundary: non-midnight MaximumDate ───────────────────────────────────

    [Fact]
    public void IsDateDisabled_BoundaryDayMatchesNonMidnightMaximumDate_ReturnsFalse()
    {
        var boundaryDay = Midnight(2025, 6, 13);
        var maximumDate = NonMidnight(2025, 6, 13);

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            boundaryDay,
            DateTime.MinValue,
            maximumDate,
            disabledSet: null);

        result.Should().BeFalse("the boundary day itself should be enabled");
    }

    // ── strictly before minimum ──────────────────────────────────────────────

    [Fact]
    public void IsDateDisabled_DayBeforeMinimumDate_ReturnsTrue()
    {
        var dayBeforeMin = Midnight(2025, 5, 13);
        var minimumDate = Midnight(2025, 5, 14);

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            dayBeforeMin,
            minimumDate,
            DateTime.MaxValue,
            disabledSet: null);

        result.Should().BeTrue();
    }

    // ── strictly after maximum ───────────────────────────────────────────────

    [Fact]
    public void IsDateDisabled_DayAfterMaximumDate_ReturnsTrue()
    {
        var dayAfterMax = Midnight(2025, 6, 14);
        var maximumDate = Midnight(2025, 6, 13);

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            dayAfterMax,
            DateTime.MinValue,
            maximumDate,
            disabledSet: null);

        result.Should().BeTrue();
    }

    // ── present in disabled set ──────────────────────────────────────────────

    [Fact]
    public void IsDateDisabled_DayPresentInDisabledSet_ReturnsTrue()
    {
        var disabledDay = Midnight(2025, 5, 20);
        var disabledSet = new HashSet<DateTime> { disabledDay };

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            disabledDay,
            DateTime.MinValue,
            DateTime.MaxValue,
            disabledSet);

        result.Should().BeTrue();
    }

    // ── day in valid range ───────────────────────────────────────────────────

    [Fact]
    public void IsDateDisabled_DayWithinRange_ReturnsFalse()
    {
        var day = Midnight(2025, 5, 20);

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            day,
            Midnight(2025, 5, 14),
            Midnight(2025, 6, 13),
            disabledSet: null);

        result.Should().BeFalse();
    }

    // ── default min/max (DateTime.MinValue / DateTime.MaxValue) ─────────────

    [Fact]
    public void IsDateDisabled_DefaultMinMaxAndNullDisabledSet_ReturnsFalse()
    {
        var anyDay = Midnight(2025, 5, 21);

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            anyDay,
            DateTime.MinValue,
            DateTime.MaxValue,
            disabledSet: null);

        result.Should().BeFalse("with default bounds every day should be enabled");
    }

    // ── theory: various time-of-day values on the boundary ───────────────────

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(12, 0, 0)]
    [InlineData(23, 59, 59)]
    public void IsDateDisabled_BoundaryDayWithAnyTimeOfDayInMinimumDate_ReturnsFalse(
        int hour, int minute, int second)
    {
        var boundaryDay = Midnight(2025, 5, 14);
        var minimumDate = new DateTime(2025, 5, 14, hour, minute, second);

        var result = global::Plugin.Maui.Calendar.Controls.Calendar.IsDateDisabled(
            boundaryDay,
            minimumDate,
            DateTime.MaxValue,
            disabledSet: null);

        result.Should().BeFalse(
            $"boundary day should be enabled regardless of MinimumDate time {hour:D2}:{minute:D2}:{second:D2}");
    }
}
