using FluentAssertions;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Interfaces;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.ViewLayoutEngines;

/// <summary>
/// Verifies how the layout engines move between units (a month, or one or two weeks) and which days
/// they show, including at both ends of the range of <see cref="DateTime"/>.
/// </summary>
public class LayoutUnitTests
{
    static readonly DateTime May15 = new(2025, 5, 15, 13, 45, 0, DateTimeKind.Local);

    public static TheoryData<string> Engines => ["month", "week", "two weeks"];

    static IViewLayoutEngine Create(string engine) => engine switch
    {
        "month" => new MonthViewEngine(DayOfWeek.Sunday),
        "week" => new WeekViewEngine(1, DayOfWeek.Sunday),
        _ => new WeekViewEngine(2, DayOfWeek.Sunday),
    };

    // ── Moving by several units ──────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(Engines))]
    public void ZeroUnits_KeepTheDate(string engine)
    {
        var layout = Create(engine);

        layout.GetNextUnit(May15, 0).Should().Be(May15);
        layout.GetPreviousUnit(May15, 0).Should().Be(May15);
    }

    [Theory]
    [MemberData(nameof(Engines))]
    public void NegativeUnits_MoveTheOtherWay(string engine)
    {
        var layout = Create(engine);

        layout.GetNextUnit(May15, -3).Should().Be(layout.GetPreviousUnit(May15, 3));
        layout.GetPreviousUnit(May15, -3).Should().Be(layout.GetNextUnit(May15, 3));
    }

    [Fact]
    public void MonthUnits_KeepTheTimeAndKindAndMoveToTheLastDayOfAShorterMonth()
    {
        var layout = new MonthViewEngine(DayOfWeek.Sunday);
        var january31 = new DateTime(2025, 1, 31, 8, 30, 0, DateTimeKind.Utc);

        var next = layout.GetNextUnit(january31, 1);

        next.Should().Be(new DateTime(2025, 2, 28, 8, 30, 0));
        next.Kind.Should().Be(DateTimeKind.Utc);
        layout.GetPreviousUnit(new DateTime(2025, 3, 31), 1).Should().Be(new DateTime(2025, 2, 28));
        layout.GetNextUnit(May15, 20).Should().Be(new DateTime(2027, 1, 15, 13, 45, 0));
    }

    [Theory]
    [InlineData(1, 7)]
    [InlineData(2, 14)]
    public void WeekUnits_MoveBySevenDaysPerWeekAndKeepTheTimeAndKind(int numberOfWeeks, int daysPerUnit)
    {
        var layout = new WeekViewEngine(numberOfWeeks, DayOfWeek.Sunday);

        var next = layout.GetNextUnit(May15, 2);

        next.Should().Be(May15.AddDays(2 * daysPerUnit));
        next.Kind.Should().Be(DateTimeKind.Local);
        layout.GetPreviousUnit(May15, 2).Should().Be(May15.AddDays(-2 * daysPerUnit));
    }

    // ── The ends of the range of DateTime ────────────────────────────────────

    [Fact]
    public void MonthUnits_StopAtTheFirstAndLastMonthOfDateTime()
    {
        var layout = new MonthViewEngine(DayOfWeek.Sunday);

        layout.GetPreviousUnit(new DateTime(1, 1, 20)).Should().Be(new DateTime(1, 1, 20));
        layout.GetPreviousUnit(new DateTime(1, 3, 20), 10).Should().Be(new DateTime(1, 1, 20));
        layout.GetNextUnit(new DateTime(9999, 12, 20)).Should().Be(new DateTime(9999, 12, 20));
        layout.GetNextUnit(new DateTime(9999, 10, 20), 10).Should().Be(new DateTime(9999, 12, 20));
    }

    [Fact]
    public void WeekUnits_StopAtTheFirstAndLastDayOfDateTime()
    {
        var layout = new WeekViewEngine(1, DayOfWeek.Sunday);

        layout.GetPreviousUnit(new DateTime(1, 1, 3, 10, 0, 0)).Should().Be(DateTime.MinValue);
        layout.GetPreviousUnit(new DateTime(1, 1, 20, 10, 0, 0), int.MaxValue).Should().Be(DateTime.MinValue);
        layout.GetNextUnit(new DateTime(9999, 12, 29, 10, 0, 0)).Should().Be(DateTime.MaxValue);
        layout.GetNextUnit(new DateTime(9999, 12, 1, 10, 0, 0), int.MaxValue).Should().Be(DateTime.MaxValue);
    }

    [Theory]
    [MemberData(nameof(Engines))]
    public void FirstDate_OfTheFirstWeekOfDateTime_IsDateTimeMinValue(string engine)
    {
        // January 1 of year 1 is a Monday: with Sunday first, its week would start a day earlier.
        var layout = Create(engine);

        layout.GetFirstDate(new DateTime(1, 1, 3)).Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void LastDate_OfTheFirstWeekOfDateTime_IsItsSaturday()
    {
        new WeekViewEngine(1, DayOfWeek.Sunday).GetLastDate(new DateTime(1, 1, 3)).Should().Be(new DateTime(1, 1, 6));
        new MonthViewEngine(DayOfWeek.Sunday).GetLastDate(new DateTime(1, 1, 3)).Should().Be(new DateTime(1, 2, 10));
    }

    [Theory]
    [MemberData(nameof(Engines))]
    public void FirstDate_FallsOnTheFirstDayOfTheWeek(string engine)
    {
        var layout = Create(engine);

        layout.GetFirstDate(May15).DayOfWeek.Should().Be(DayOfWeek.Sunday);
        layout.GetFirstDate(May15).TimeOfDay.Should().Be(TimeSpan.Zero);
    }
}
