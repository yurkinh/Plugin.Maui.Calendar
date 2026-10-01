using System.Collections;
using System.Globalization;
using FluentAssertions;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Controls.SelectionEngines;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.SelectionEngines;

/// <summary>
/// Verifies the selection engines on their own: which dates a tap selects, which dates count as
/// selected, the selected-date text and the events of the selected dates.
/// </summary>
public class SelectionEngineTests
{
    static readonly CultureInfo invariant = CultureInfo.InvariantCulture;
    static readonly CultureInfo arabic = new("ar-EG");

    static DateTime May(int day, int hour = 0) => new(2025, 5, day, hour, 0, 0);

    static EventCollection EventsOn(params int[] days)
    {
        var events = new EventCollection();
        foreach (var day in days)
        {
            events.Add(May(day), new List<string> { $"event {day}" });
        }

        return events;
    }

    static List<object> Items(ICollection collection) => [.. collection.Cast<object>()];

    // ── Single selection ─────────────────────────────────────────────────────

    [Fact]
    public void Single_TapSelectsTheDayAndTappingItAgainDeselectsIt()
    {
        ISelectionEngine engine = new SingleSelectionEngine();

        engine.PerformDateSelection(May(10)).Should().Equal(May(10));
        engine.IsDateSelected(May(10, 15)).Should().BeTrue();
        engine.IsDateSelected(May(11)).Should().BeFalse();

        engine.PerformDateSelection(May(10)).Should().BeEmpty();
        engine.IsDateSelected(May(10)).Should().BeFalse();
    }

    [Fact]
    public void Single_DateSelectedFromCodeWithATime_IsDeselectedByATapOnItsDay()
    {
        ISelectionEngine engine = new SingleSelectionEngine();
        engine.UpdateDateSelection([May(10, 15)]);

        engine.PerformDateSelection(May(10)).Should().BeEmpty();
    }

    [Fact]
    public void Single_TapOnADisabledDay_ClearsTheSelection()
    {
        ISelectionEngine engine = new SingleSelectionEngine();
        engine.PerformDateSelection(May(10));

        // A disabled date given with a time still disables its day.
        engine.PerformDateSelection(May(12), [May(12, 9)]).Should().BeEmpty();

        engine.IsDateSelected(May(10)).Should().BeFalse();
    }

    [Fact]
    public void Single_UpdateDateSelection_SelectsTheFirstDateOrNothing()
    {
        ISelectionEngine engine = new SingleSelectionEngine();

        engine.UpdateDateSelection([May(3), May(4)]);
        engine.IsDateSelected(May(3)).Should().BeTrue();
        engine.IsDateSelected(May(4)).Should().BeFalse();

        engine.UpdateDateSelection([]);
        engine.IsDateSelected(May(3)).Should().BeFalse();

        engine.UpdateDateSelection([May(3)]);
        engine.UpdateDateSelection(null!);
        engine.IsDateSelected(May(3)).Should().BeFalse();
    }

    [Fact]
    public void Single_SelectedDateText_FormatsTheSelectedDateOrIsNull()
    {
        ISelectionEngine engine = new SingleSelectionEngine();
        engine.GetSelectedDateText("d MMM", invariant, false).Should().BeNull();

        engine.UpdateDateSelection([May(7)]);

        engine.GetSelectedDateText("d MMM", invariant, false).Should().Be("7 May");
        engine.GetSelectedDateText("%d", arabic, true).Should().Be("٧");
    }

    [Fact]
    public void Single_SelectedEvents_AreTheEventsOfTheSelectedDay()
    {
        ISelectionEngine engine = new SingleSelectionEngine();
        var events = EventsOn(7);

        engine.TryGetSelectedEvents(events, out var none).Should().BeFalse();
        none.Should().BeNull();

        engine.UpdateDateSelection([May(7)]);
        engine.TryGetSelectedEvents(events, out var selected).Should().BeTrue();
        Items(selected).Should().Equal("event 7");

        engine.UpdateDateSelection([May(8)]);
        engine.TryGetSelectedEvents(events, out _).Should().BeFalse();
    }

    // ── Multiple selection ───────────────────────────────────────────────────

    [Fact]
    public void Multi_TapsToggleDaysAndDisabledDaysAreNotSelected()
    {
        ISelectionEngine engine = new MultiSelectionEngine();

        engine.PerformDateSelection(May(1, 10));
        engine.PerformDateSelection(May(3));
        engine.PerformDateSelection(May(5), [May(5, 18)]).Should().BeEquivalentTo([May(1), May(3)]);

        engine.PerformDateSelection(May(1)).Should().Equal(May(3));
        engine.IsDateSelected(May(3, 22)).Should().BeTrue();
        engine.IsDateSelected(May(1)).Should().BeFalse();
    }

    [Fact]
    public void Multi_DatesSelectedFromCodeWithATime_CountByDay()
    {
        ISelectionEngine engine = new MultiSelectionEngine();

        engine.UpdateDateSelection([May(2, 9), May(4, 17)]);

        engine.IsDateSelected(May(2)).Should().BeTrue();
        engine.IsDateSelected(May(4)).Should().BeTrue();
        engine.PerformDateSelection(May(2)).Should().Equal(May(4));

        engine.UpdateDateSelection(null!);
        engine.IsDateSelected(May(4)).Should().BeFalse();
    }

    [Fact]
    public void Multi_SelectedDateText_JoinsTheSelectedDates()
    {
        ISelectionEngine engine = new MultiSelectionEngine();
        engine.GetSelectedDateText("%d", invariant, false).Should().BeEmpty();

        engine.UpdateDateSelection([May(2), May(4), DateTime.MinValue]);

        engine.GetSelectedDateText("%d", invariant, false).Should().Be("2, 4", "DateTime.MinValue stands for no date");
        engine.GetSelectedDateText("%d", arabic, true).Should().Be("٢, ٤");
    }

    [Fact]
    public void Multi_SelectedEvents_AreTheEventsOfAllSelectedDays()
    {
        ISelectionEngine engine = new MultiSelectionEngine();
        engine.UpdateDateSelection([May(2), May(3), May(4)]);

        engine.TryGetSelectedEvents(EventsOn(2, 4, 9), out var selected).Should().BeTrue();

        Items(selected).Should().Equal("event 2", "event 4");
    }

    // ── Range selection ──────────────────────────────────────────────────────

    [Fact]
    public void Range_FirstTapSelectsOneDayAndSecondTapExtendsTheRange()
    {
        var engine = new RangedSelectionEngine();
        ISelectionEngine selection = engine;

        selection.PerformDateSelection(May(10)).Should().Equal(May(10));
        selection.PerformDateSelection(May(13, 20)).Should().Equal(May(10), May(11), May(12), May(13));
        engine.RangeSelectionStartDate.Should().Be(May(10));
        engine.RangeSelectionEndDate.Should().Be(May(13));

        // A complete range: the next tap starts a new one.
        selection.PerformDateSelection(May(20)).Should().Equal(May(20));
    }

    [Fact]
    public void Range_SecondTapBeforeTheFirst_ExtendsTheRangeBackwards()
    {
        ISelectionEngine engine = new RangedSelectionEngine();

        engine.PerformDateSelection(May(10));
        engine.PerformDateSelection(May(8)).Should().Equal(May(8), May(9), May(10));
    }

    [Fact]
    public void Range_DisabledDaysAreLeftOutOfTheRange()
    {
        ISelectionEngine engine = new RangedSelectionEngine();

        engine.PerformDateSelection(May(10));
        engine.PerformDateSelection(May(12), [May(11, 8)]).Should().Equal(May(10), May(12));
        engine.PerformDateSelection(May(15), null).Should().Equal(May(15));
    }

    [Fact]
    public void Range_SelectingNothing_ClearsTheRange()
    {
        var engine = new RangedSelectionEngine();
        engine.SelectDateRange(May(10), null);

        engine.SelectDateRange(null, null).Should().BeEmpty();

        engine.RangeSelectionStartDate.Should().BeNull();
        engine.RangeSelectionEndDate.Should().BeNull();
        engine.GetDateRange().Should().BeEmpty();
    }

    [Fact]
    public void Range_UpdateDateSelection_SpansTheEarliestToTheLatestDate()
    {
        ISelectionEngine engine = new RangedSelectionEngine();

        engine.UpdateDateSelection([May(12, 8), May(5), May(9)]);

        engine.IsDateSelected(May(4)).Should().BeFalse();
        engine.IsDateSelected(May(5)).Should().BeTrue();
        engine.IsDateSelected(May(12, 23)).Should().BeTrue();
        engine.IsDateSelected(May(13)).Should().BeFalse();

        engine.UpdateDateSelection([]);
        engine.IsDateSelected(May(5)).Should().BeFalse();

        engine.UpdateDateSelection(null!);
        engine.IsDateSelected(May(5)).Should().BeFalse();
    }

    [Fact]
    public void Range_SelectedDateText_ShowsBothEnds()
    {
        ISelectionEngine engine = new RangedSelectionEngine();
        engine.GetSelectedDateText("%d", invariant, false).Should().BeEmpty();

        engine.UpdateDateSelection([May(2), May(5)]);

        engine.GetSelectedDateText("d MMM", invariant, false).Should().Be("2 May - 5 May");
        engine.GetSelectedDateText("%d", arabic, true).Should().Be("٢ - ٥");
    }

    [Fact]
    public void Range_SelectedEvents_AreTheEventsOfTheWholeRange()
    {
        ISelectionEngine engine = new RangedSelectionEngine();
        engine.UpdateDateSelection([May(2), May(5)]);

        engine.TryGetSelectedEvents(EventsOn(1, 3, 5, 6), out var selected).Should().BeTrue();

        Items(selected).Should().Equal("event 3", "event 5");
    }

    [Fact]
    public void Range_EndingOnTheLastDayOfDateTime_DoesNotOverflow()
    {
        var engine = new RangedSelectionEngine();
        var lastDay = DateTime.MaxValue.Date;

        engine.SelectDateRange(lastDay.AddDays(-1), null);

        engine.SelectDateRange(lastDay, null).Should().Equal(lastDay.AddDays(-1), lastDay);
    }

    // ── Week selection ───────────────────────────────────────────────────────

    static ISelectionEngine WeekEngine(DayOfWeek firstDayOfWeek = DayOfWeek.Sunday) =>
        new WeekSelectionEngine(() => firstDayOfWeek);

    [Fact]
    public void Week_TapSelectsTheWholeWeekAndTappingItAgainDeselectsIt()
    {
        var engine = WeekEngine(DayOfWeek.Monday);

        // Thursday May 15 2025: its Monday-first week runs from May 12 to May 18.
        engine.PerformDateSelection(May(15)).Should().Equal(Enumerable.Range(12, 7).Select(day => May(day)));
        engine.IsDateSelected(May(18, 23)).Should().BeTrue();
        engine.IsDateSelected(May(19)).Should().BeFalse();

        engine.PerformDateSelection(May(12)).Should().BeEmpty();
        engine.IsDateSelected(May(15)).Should().BeFalse();
    }

    [Fact]
    public void Week_FollowsTheFirstDayOfWeekWhenADayIsTapped()
    {
        var firstDayOfWeek = DayOfWeek.Sunday;
        ISelectionEngine engine = new WeekSelectionEngine(() => firstDayOfWeek);

        engine.PerformDateSelection(May(15)).First().Should().Be(May(11));

        firstDayOfWeek = DayOfWeek.Monday;
        engine.PerformDateSelection(May(20)).First().Should().Be(May(19));
    }

    [Fact]
    public void Week_DisabledDays_AreLeftOutAndTappingOneClearsTheSelection()
    {
        var engine = WeekEngine();

        engine.PerformDateSelection(May(15), [May(13, 7)]).Should().Equal(May(11), May(12), May(14), May(15), May(16), May(17));

        engine.PerformDateSelection(May(13), [May(13)]).Should().BeEmpty();
        engine.IsDateSelected(May(15)).Should().BeFalse();
    }

    [Fact]
    public void Week_UpdateDateSelection_SelectsTheWeekOfTheFirstDate()
    {
        var engine = WeekEngine();

        engine.UpdateDateSelection([May(21, 10), May(2)]);
        engine.IsDateSelected(May(18)).Should().BeTrue();
        engine.IsDateSelected(May(24)).Should().BeTrue();
        engine.IsDateSelected(May(2)).Should().BeFalse();

        engine.UpdateDateSelection([]);
        engine.IsDateSelected(May(21)).Should().BeFalse();

        engine.UpdateDateSelection([May(21)]);
        engine.UpdateDateSelection(null!);
        engine.IsDateSelected(May(21)).Should().BeFalse();
    }

    [Fact]
    public void Week_SelectedDateText_ShowsTheFirstAndLastDay()
    {
        var engine = WeekEngine();
        engine.GetSelectedDateText("%d", invariant, false).Should().BeEmpty();

        engine.UpdateDateSelection([May(15)]);

        engine.GetSelectedDateText("d MMM", invariant, false).Should().Be("11 May - 17 May");
        engine.GetSelectedDateText("%d", arabic, true).Should().Be("١١ - ١٧");
    }

    [Fact]
    public void Week_SelectedEvents_AreTheEventsOfTheWeek()
    {
        var engine = WeekEngine();
        engine.TryGetSelectedEvents(EventsOn(12), out _).Should().BeFalse();

        engine.UpdateDateSelection([May(15)]);

        engine.TryGetSelectedEvents(EventsOn(10, 12, 17, 18), out var selected).Should().BeTrue();
        Items(selected).Should().Equal("event 12", "event 17");
    }

    [Fact]
    public void Week_FirstAndLastWeekOfDateTime_StopAtItsEnds()
    {
        var engine = WeekEngine();

        // January 1 of year 1 is a Monday; December 31 of 9999 is a Friday.
        engine.PerformDateSelection(DateTime.MinValue).Should().Equal(Enumerable.Range(0, 6).Select(day => DateTime.MinValue.AddDays(day)));
        engine.PerformDateSelection(DateTime.MaxValue).Should().Equal(Enumerable.Range(0, 6).Select(day => new DateTime(9999, 12, 26).AddDays(day)));

        engine.UpdateDateSelection([DateTime.MinValue]);
        engine.IsDateSelected(DateTime.MinValue).Should().BeTrue("DateTime.MinValue is a day that can be selected");
    }
}
