using System.Collections;
using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies the <see cref="ICalendarDay"/> state that the calendar assigns to its day cells (the
/// binding context of a <c>DayViewTemplate</c>) and the PropertyChanged notifications templates
/// rely on when cells are reused.
/// </summary>
/// <remarks>
/// Dates are fixed (the May 2025 grid runs from Apr 27 to Jun 7) except in the IsToday tests,
/// which run through <see cref="CalendarTestExtensions.OnStableDay"/> so that crossing midnight
/// cannot make them fail.
/// </remarks>
[Collection(MauiControlsCollection.Name)]
public class CalendarDayStateTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static DateTime May(int day) => new(2025, 5, day);

    static List<DateTime> Dates(IEnumerable<DayModel> days) => [.. days.Select(day => day.Date)];

    // ── IsSelected ───────────────────────────────────────────────────────────

    [Fact]
    public void IsSelected_FollowsSelectedDateAndRaisesPropertyChanged()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var may12 = calendar.DayFor(May(12));
        var may12Changes = may12.RecordPropertyChanges();

        calendar.SelectedDate = May(12);

        Dates(calendar.Days().Where(day => day.IsSelected)).Should().Equal(May(12));
        may12Changes.Should().Contain(nameof(ICalendarDay.IsSelected));

        may12Changes.Clear();
        calendar.SelectedDate = May(13);

        may12.IsSelected.Should().BeFalse();
        may12Changes.Should().Contain(nameof(ICalendarDay.IsSelected));
        Dates(calendar.Days().Where(day => day.IsSelected)).Should().Equal(May(13));
    }

    // ── IsDisabled ───────────────────────────────────────────────────────────

    [Fact]
    public void IsDisabled_OutsideMinimumAndMaximumDateOrInDisabledDates()
    {
        // ShownDate is set last: it forces the day update that applies the other properties.
        var calendar = new TestCalendar
        {
            MinimumDate = May(5),
            MaximumDate = May(25),
            DisabledDates = [May(20)],
            ShownDate = May15,
        };

        foreach (var day in calendar.Days())
        {
            var expected = day.Date < May(5) || day.Date > May(25) || day.Date == May(20);
            day.IsDisabled.Should().Be(expected, $"{day.Date:d}");
        }
    }

    [Fact]
    public void IsDisabled_DisabledDatesAssignedAtRuntime_UpdatesVisibleCells()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var may20 = calendar.DayFor(May(20));
        var changes = may20.RecordPropertyChanges();

        calendar.DisabledDates = [May(20)];

        may20.IsDisabled.Should().BeTrue("the visible cells must follow a new DisabledDates list");
        changes.Should().Contain(nameof(ICalendarDay.IsDisabled));

        calendar.DisabledDates = [];

        may20.IsDisabled.Should().BeFalse();
    }

    [Fact]
    public void IsDisabled_MinimumAndMaximumDateChangedAtRuntime_UpdateVisibleCells()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.MinimumDate = May(10);
        calendar.MaximumDate = May(20);

        calendar.DayFor(May(9)).IsDisabled.Should().BeTrue("the visible cells must follow a new MinimumDate");
        calendar.DayFor(May(10)).IsDisabled.Should().BeFalse();
        calendar.DayFor(May(20)).IsDisabled.Should().BeFalse();
        calendar.DayFor(May(21)).IsDisabled.Should().BeTrue("the visible cells must follow a new MaximumDate");
    }

    [Fact]
    public void DisabledDatesAndEvents_DefaultValuesAreNotSharedBetweenCalendars()
    {
        var first = new TestCalendar { ShownDate = May15 };
        var second = new TestCalendar { ShownDate = May15 };

        first.DisabledDates.Should().NotBeSameAs(second.DisabledDates);
        first.Events.Should().NotBeSameAs(second.Events);
    }

    [Fact]
    public void Events_DefaultCollectionChangedAtRuntime_UpdatesOnlyItsOwnCalendar()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var other = new TestCalendar { ShownDate = May15 };

        calendar.Events.Add(May(12), new List<string> { "a" });

        calendar.DayFor(May(12)).HasEvents.Should().BeTrue("the default Events collection is observed too");
        other.Events.Should().BeEmpty();
        other.DayFor(May(12)).HasEvents.Should().BeFalse();
    }

    // ── HasEvents, EventCount, EventColors ───────────────────────────────────

    [Fact]
    public void EventState_ReflectsEachDaysEntry()
    {
        var multiColor = new MultiColorEvents(Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Orange, Colors.Purple)
        {
            "event",
        };
        var calendar = new TestCalendar
        {
            EventIndicatorColor = Colors.Red,
            EventIndicatorSelectedColor = Colors.Blue,
            Events = new EventCollection
            {
                [May(10)] = new List<string> { "a", "b", "c" },
                [May(11)] = new List<string>(),
                [May(12)] = multiColor,
            },
            ShownDate = May15,
        };

        ICalendarDay threeEvents = calendar.DayFor(May(10));
        threeEvents.HasEvents.Should().BeTrue();
        threeEvents.EventCount.Should().Be(3);
        threeEvents.Events.Should().Equal("a", "b", "c");
        threeEvents.EventColors.Should().Equal(Colors.Red);

        ICalendarDay emptyEntry = calendar.DayFor(May(11));
        emptyEntry.HasEvents.Should().BeTrue("the day has an entry, even though it is empty");
        emptyEntry.EventCount.Should().Be(0);
        emptyEntry.Events.Should().BeEmpty();

        ICalendarDay multiColorDay = calendar.DayFor(May(12));
        multiColorDay.EventCount.Should().Be(1);
        multiColorDay.EventColors.Should().Equal(multiColor.Colors.Take(5), "at most five indicator colors are shown");

        ICalendarDay noEvents = calendar.DayFor(May(13));
        noEvents.HasEvents.Should().BeFalse();
        noEvents.EventCount.Should().Be(0);
        noEvents.Events.Should().BeEmpty();
        noEvents.EventColors.Should().BeEmpty();

        calendar.SelectedDate = May(10);

        threeEvents.EventColors.Should().Equal(Colors.Blue);
    }

    [Fact]
    public void EventColors_MultiEventDayWithoutColors_ShowsSingleIndicatorColor()
    {
        var calendar = new TestCalendar
        {
            EventIndicatorColor = Colors.Red,
            Events = new EventCollection
            {
                [May(10)] = new MultiColorEvents(null!) { "no colors" },
                [May(11)] = new MultiColorEvents() { "empty colors" },
            },
            ShownDate = May15,
        };

        calendar.DayFor(May(10)).EventColors.Should().Equal([Colors.Red], "a day with events always shows a dot");
        calendar.DayFor(May(11)).EventColors.Should().Equal([Colors.Red]);
    }

    [Fact]
    public void EventState_EventsAddedOrRemovedAtRuntime_RefreshesCellsAndRaisesPropertyChanged()
    {
        var events = new EventCollection();
        var calendar = new TestCalendar { ShownDate = May15, Events = events };
        var may12 = calendar.DayFor(May(12));
        var changes = may12.RecordPropertyChanges();

        events.Add(May(12), new List<string> { "a", "b" });

        may12.HasEvents.Should().BeTrue();
        may12.EventCount.Should().Be(2);
        may12.Events.Should().Equal("a", "b");
        changes.Should().Contain([
            nameof(ICalendarDay.HasEvents),
            nameof(ICalendarDay.EventCount),
            nameof(ICalendarDay.Events),
            nameof(ICalendarDay.EventColors),
        ]);

        events[May(12)] = new List<string> { "a" };

        may12.EventCount.Should().Be(1);
        may12.Events.Should().Equal("a");

        changes.Clear();
        events.Remove(May(12));

        may12.HasEvents.Should().BeFalse();
        may12.EventCount.Should().Be(0);
        may12.Events.Should().BeEmpty();
        may12.EventColors.Should().BeEmpty();
        changes.Should().Contain([
            nameof(ICalendarDay.HasEvents),
            nameof(ICalendarDay.EventCount),
            nameof(ICalendarDay.Events),
        ]);
    }

    [Fact]
    public void EventCount_ChangeInsideADaysCollection_RefreshedByAssigningTheEntryAgain()
    {
        // Documented behavior: only changes to the EventCollection itself are observed.
        var dayEvents = new List<string> { "a" };
        var events = new EventCollection { [May(12)] = dayEvents };
        var calendar = new TestCalendar { Events = events, ShownDate = May15 };

        dayEvents.Add("b");

        calendar.DayFor(May(12)).EventCount.Should().Be(1, "a change inside a day's own collection is not observed");
        calendar.DayFor(May(12)).Events.Should().Equal("a");

        var changes = calendar.DayFor(May(12)).RecordPropertyChanges();
        events[May(12)] = dayEvents;

        calendar.DayFor(May(12)).EventCount.Should().Be(2);
        calendar.DayFor(May(12)).Events.Should().Equal("a", "b");
        changes.Should().Contain(nameof(ICalendarDay.Events),
            "assigning the same collection again must still refresh a template bound to Events");
    }

    [Fact]
    public void Events_UnchangedItems_AreNotReassignedByLaterDayUpdates()
    {
        var events = new EventCollection { [May(12)] = new List<string> { "a", "b" } };
        var calendar = new TestCalendar { Events = events, ShownDate = May15 };
        var may12 = calendar.DayFor(May(12));
        var snapshot = may12.Events;
        var changes = may12.RecordPropertyChanges();

        events.Add(May(20), new List<string> { "c" });
        calendar.SelectedDate = May(12);

        may12.Events.Should().BeSameAs(snapshot, "a BindableLayout bound to Events must not be rebuilt for nothing");
        changes.Should().NotContain(nameof(ICalendarDay.Events));
    }

    [Fact]
    public void Events_ItemsAreTheObjectsStoredInTheEventCollection()
    {
        var first = new object();
        var second = new object();
        var calendar = new TestCalendar
        {
            Events = new EventCollection { [May(12)] = new List<object> { first, second } },
            ShownDate = May15,
        };

        ICalendarDay may12 = calendar.DayFor(May(12));

        may12.Events.Should().HaveCount(2);
        may12.Events[0].Should().BeSameAs(first);
        may12.Events[1].Should().BeSameAs(second);
    }

    [Fact]
    public void EventColors_UnchangedColors_AreNotReassignedByLaterDayUpdates()
    {
        var events = new EventCollection
        {
            [May(10)] = new List<string> { "a" },
            [May(11)] = new MultiColorEvents(Colors.Red, Colors.Green) { "b" },
        };
        var calendar = new TestCalendar
        {
            EventIndicatorColor = Colors.Red,
            EventIndicatorSelectedColor = Colors.Blue,
            Events = events,
            ShownDate = May15,
        };
        var singleColor = calendar.DayFor(May(10));
        var multiColor = calendar.DayFor(May(11));
        var colorsBefore = (singleColor.EventColors, multiColor.EventColors);
        var changes = singleColor.RecordPropertyChanges();
        changes.AddRange(multiColor.RecordPropertyChanges());

        // Each of these runs a full day update without changing the colors of May 10 or May 11.
        events.Add(May(12), new List<string> { "c" });
        calendar.SelectedDate = May(20);
        calendar.DayViewSize = 44;

        changes.Should().NotContain(nameof(ICalendarDay.EventColors),
            "an unchanged list must not make the event dots be rebuilt");
        singleColor.EventColors.Should().BeSameAs(colorsBefore.Item1);
        multiColor.EventColors.Should().BeSameAs(colorsBefore.Item2);

        calendar.SelectedDate = May(10);

        singleColor.EventColors.Should().Equal(Colors.Blue);
        changes.Should().Contain(nameof(ICalendarDay.EventColors), "selecting the day changes its indicator color");
    }

    [Fact]
    public void Events_ObservedAgainAfterHandlerIsRemovedAndAttachedAgain()
    {
        var events = new EventCollection();
        var calendar = new TestCalendar { Events = events, ShownDate = May15 };

        // A cached page that is popped (handler removed) and shown again (new handler).
        calendar.SimulateHandlerAttached();
        calendar.SimulateHandlerDetached();
        events.Add(May(12), new List<string> { "added while detached" });
        calendar.SimulateHandlerAttached();

        calendar.DayFor(May(12)).HasEvents.Should().BeTrue("changes made while detached are picked up on attach");

        events.Add(May(13), new List<string> { "added after attach" });

        calendar.DayFor(May(13)).HasEvents.Should().BeTrue("Events is observed again");
        calendar.SimulateHandlerDetached();
    }

    // ── IsWeekend and IsThisMonth ────────────────────────────────────────────

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Saturday)]
    public void IsWeekend_IsSaturdayOrSundayWhateverTheFirstDayOfWeekAndWeekendColor(DayOfWeek firstDayOfWeek)
    {
        // WeekendDayColor is left at its default (Transparent): IsWeekend must not depend on it.
        var calendar = new TestCalendar { FirstDayOfWeek = firstDayOfWeek, ShownDate = May15 };

        calendar.Days().Should().OnlyContain(day =>
            day.IsWeekend == (day.Date.DayOfWeek == DayOfWeek.Saturday || day.Date.DayOfWeek == DayOfWeek.Sunday));
        calendar.Days().Count(day => day.IsWeekend).Should().Be(12, "six weeks with two weekend days each");
    }

    [Fact]
    public void IsThisMonth_InMonthLayout_OnlyForShownMonth()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        Dates(calendar.Days().Where(day => day.IsThisMonth))
            .Should().Equal(Enumerable.Range(1, 31).Select(May));
    }

    // ── IsToday ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsToday_ShownMonthWithoutToday_NoCellIsToday()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.Days().Should().OnlyContain(day => !day.IsToday);
    }

    [Fact]
    public void IsToday_ShownMonthWithToday_OnlyTodaysCell() => CalendarTestExtensions.OnStableDay(today =>
    {
        var calendar = new TestCalendar { ShownDate = today };

        Dates(calendar.Days().Where(day => day.IsToday)).Should().Equal(today);
    });

    [Fact]
    public void IsToday_StaleAfterMidnight_CorrectedByNextDayUpdate() => CalendarTestExtensions.OnStableDay(today =>
    {
        var events = new EventCollection();
        var calendar = new TestCalendar { ShownDate = today, Events = events };
        var todayCell = calendar.DayFor(today);
        var previousToday = calendar.Days().First(day => day.Date != today);

        // Put both cells in the state they had while another visible day was today: neither
        // cell's date changes on the next update, so only an explicit re-check can fix them.
        todayCell.RefreshIsToday(previousToday.Date);
        previousToday.RefreshIsToday(previousToday.Date);
        todayCell.IsToday.Should().BeFalse();
        previousToday.IsToday.Should().BeTrue();
        var changes = todayCell.RecordPropertyChanges();

        // Any change that updates the days, here an event added at runtime.
        events.Add(today.AddDays(3), new List<string> { "event" });

        todayCell.IsToday.Should().BeTrue();
        previousToday.IsToday.Should().BeFalse();
        changes.Should().Contain([
            nameof(ICalendarDay.IsToday),
            nameof(DayModel.BackgroundColor),
            nameof(DayModel.OutlineColor),
            nameof(DayModel.TextColor),
        ]);
    });

    [Fact]
    public void RefreshToday_CorrectsStaleCellsOnly() => CalendarTestExtensions.OnStableDay(today =>
    {
        var calendar = new TestCalendar { ShownDate = today };
        var todayCell = calendar.DayFor(today);
        var otherCell = calendar.Days().First(day => day.Date != today);
        todayCell.RefreshIsToday(today.AddDays(-1));
        var todayChanges = todayCell.RecordPropertyChanges();
        var otherChanges = otherCell.RecordPropertyChanges();

        calendar.RefreshToday();

        todayCell.IsToday.Should().BeTrue();
        todayChanges.Should().Contain(nameof(ICalendarDay.IsToday));
        otherChanges.Should().BeEmpty("a cell whose IsToday did not change raises nothing");
        Dates(calendar.Days().Where(day => day.IsToday)).Should().Equal(today);
    });

    [Fact]
    public void TodayRefreshTimer_RunsOnlyWhileLoadedAndRefreshesOnTick() => CalendarTestExtensions.OnStableDay(today =>
    {
        var calendar = new TestCalendar { ShownDate = today };
        var todayCell = calendar.DayFor(today);
        var timers = SyncDispatcher.Instance.CreatedTimers;
        var timerCountBefore = timers.Count;

        todayCell.RefreshIsToday(today.AddDays(-1));
        calendar.SimulateLoaded();

        todayCell.IsToday.Should().BeTrue("loading re-checks today, the calendar may have been off screen at midnight");
        timers.Should().HaveCount(timerCountBefore + 1);
        var timer = timers[^1];
        timer.IsRunning.Should().BeTrue();
        timer.Interval.Should().BeGreaterThan(TimeSpan.Zero).And.BeLessThanOrEqualTo(TimeSpan.FromHours(1));
        timer.TickSubscriberCount.Should().Be(1);

        // Several ticks in a row: MAUI stops a one-shot timer after its Tick handlers return, so a
        // timer that only re-arms itself inside Tick would never tick a second time.
        for (var tick = 1; tick <= 3; tick++)
        {
            todayCell.RefreshIsToday(today.AddDays(-1));
            timer.Fire();

            todayCell.IsToday.Should().BeTrue($"tick {tick} re-checks today");
            timer.IsRunning.Should().BeTrue($"the timer keeps running after tick {tick}");
            timer.Interval.Should().BeGreaterThan(TimeSpan.Zero).And.BeLessThanOrEqualTo(TimeSpan.FromHours(1));
        }

        calendar.SimulateUnloaded();

        timer.IsRunning.Should().BeFalse();
        timer.TickSubscriberCount.Should().Be(0, "an unloaded calendar must not be kept alive by its timer");

        calendar.SimulateLoaded();
        var secondTimer = timers[^1];
        secondTimer.Should().NotBeSameAs(timer);
        secondTimer.IsRunning.Should().BeTrue();

        calendar.Dispose();

        secondTimer.IsRunning.Should().BeFalse();
        secondTimer.TickSubscriberCount.Should().Be(0);
    });

    [Theory]
    [InlineData(23, 59, 59, 500, 1_500)]
    [InlineData(23, 30, 0, 0, 1_801_000)]
    [InlineData(23, 0, 0, 0, 3_600_000)]
    [InlineData(12, 0, 0, 0, 3_600_000)]
    [InlineData(0, 0, 0, 0, 3_600_000)]
    public void GetTodayRefreshInterval_FiresJustAfterMidnightButWaitsAtMostOneHour(
        int hour, int minute, int second, int millisecond, int expectedMilliseconds)
    {
        var now = new DateTime(2025, 5, 10, hour, minute, second, millisecond);

        CalendarControl.GetTodayRefreshInterval(now)
            .Should().Be(TimeSpan.FromMilliseconds(expectedMilliseconds));
    }

    // ── IsRangeStart and IsRangeEnd ──────────────────────────────────────────

    static TestRangeSelectionCalendar CreateRangeCalendar(DateTime start, int days)
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15 };
        calendar.SelectedDates = new ObservableCollection<DateTime>(Enumerable.Range(0, days).Select(offset => start.AddDays(offset)));
        return calendar;
    }

    [Fact]
    public void RangeFlags_MarkFirstAndLastDayOfSelectedRange()
    {
        var calendar = CreateRangeCalendar(May(10), days: 5);

        Dates(calendar.Days().Where(day => day.IsSelected)).Should().Equal(Enumerable.Range(10, 5).Select(May),
            "every day of the range, including both ends, is selected");
        Dates(calendar.Days().Where(day => day.IsRangeStart)).Should().Equal(May(10));
        Dates(calendar.Days().Where(day => day.IsRangeEnd)).Should().Equal(May(14));
    }

    [Fact]
    public void RangeFlags_ResetWhenCellsMoveToOtherDatesAndRestoredOnReturn()
    {
        var calendar = CreateRangeCalendar(May(10), days: 5);
        var startCell = calendar.DayFor(May(10));
        var startCellChanges = startCell.RecordPropertyChanges();

        calendar.NextLayoutUnitCommand.Execute(null);

        calendar.Days().Should().OnlyContain(day => !day.IsRangeStart && !day.IsRangeEnd,
            "the June 2025 grid shows no day of the range");
        startCellChanges.Should().Contain(nameof(ICalendarDay.IsRangeStart));

        calendar.PrevLayoutUnitCommand.Execute(null);

        Dates(calendar.Days().Where(day => day.IsRangeStart)).Should().Equal(May(10));
        Dates(calendar.Days().Where(day => day.IsRangeEnd)).Should().Equal(May(14));
    }

    [Fact]
    public void RangeFlags_ClearedByClearSelection()
    {
        var calendar = CreateRangeCalendar(May(10), days: 5);

        calendar.ClearSelection();

        calendar.Days().Should().OnlyContain(day => !day.IsSelected && !day.IsRangeStart && !day.IsRangeEnd);
    }

    [Fact]
    public void RangeColors_KeptAfterGlobalColorChangeAndFollowRuntimeRangeColorChange()
    {
        var calendar = new TestRangeSelectionCalendar
        {
            ShownDate = May15,
            SelectedDayBackgroundColor = Colors.Blue,
            SelectedDatesRangeBackgroundColor = Colors.LightBlue,
        };
        calendar.SelectedDates = new ObservableCollection<DateTime>(Enumerable.Range(10, 5).Select(May));
        var start = calendar.DayFor(May(10));
        var middle = calendar.DayFor(May(12));
        middle.SelectedBackgroundColor.Should().Be(Colors.LightBlue);

        // Any global color change (or a theme switch through AppThemeBinding) resets every cell's
        // SelectedBackgroundColor to SelectedDayBackgroundColor first.
        calendar.DisabledDayColor = Colors.Gray;

        middle.SelectedBackgroundColor.Should().Be(Colors.LightBlue, "the range keeps its own color");
        start.SelectedBackgroundColor.Should().Be(Colors.Blue, "the range ends keep the selected day color");

        calendar.SelectedDatesRangeBackgroundColor = Colors.Orange;

        middle.SelectedBackgroundColor.Should().Be(Colors.Orange, "a new range color is applied right away");
        start.SelectedBackgroundColor.Should().Be(Colors.Blue);
    }

    [Fact]
    public void SelectedEndDateSetFromCode_AppliedAfterRangeWasExtendedAtItsStart()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15 };
        calendar.WithTapHandling(calendar.CellFor(May(14)).Tap);
        calendar.WithTapHandling(calendar.CellFor(May(10)).Tap);
        calendar.SelectedStartDate.Should().Be(May(10), "tapping an earlier day extends the range at its start");
        calendar.SelectedEndDate.Should().Be(May(14));

        calendar.SelectedEndDate = May(20);

        calendar.DayFor(May(20)).IsSelected.Should().BeTrue("a SelectedEndDate set from code must reach the selection");
        Dates(calendar.Days().Where(day => day.IsRangeEnd)).Should().Equal(May(20));
    }

    [Fact]
    public void RangeFlags_AlwaysFalseOutsideRangeSelectionCalendar()
    {
        var single = new TestCalendar { ShownDate = May15, SelectedDate = May(10) };
        var multi = new TestMultiSelectionCalendar { ShownDate = May15 };
        multi.SelectedDates = [May(10), May(11), May(12)];

        single.Days().Should().Contain(day => day.IsSelected);
        single.Days().Should().OnlyContain(day => !day.IsRangeStart && !day.IsRangeEnd);
        multi.Days().Count(day => day.IsSelected).Should().Be(3);
        multi.Days().Should().OnlyContain(day => !day.IsRangeStart && !day.IsRangeEnd);
    }

    // ── notifications when a cell is reused ──────────────────────────────────

    [Fact]
    public void EveryChangedCalendarDayMember_RaisesPropertyChanged_WhenCellsMoveToAnotherMonth()
    {
        // June has a selected range, events and a disabled day, so most members change when the
        // reused cells move from the May grid to the June grid.
        var calendar = new TestRangeSelectionCalendar
        {
            DisabledDates = [new DateTime(2025, 6, 20)],
            Events = new EventCollection { [new DateTime(2025, 6, 5)] = new List<string> { "a", "b" } },
            ShownDate = May15,
        };
        calendar.SelectedDates = new ObservableCollection<DateTime>(
            Enumerable.Range(10, 5).Select(day => new DateTime(2025, 6, day)));
        var members = typeof(ICalendarDay).GetProperties();
        var before = calendar.Days().Select(day => members.Select(member => member.GetValue(day)).ToArray()).ToList();
        var changes = calendar.Days().Select(day => day.RecordPropertyChanges()).ToList();

        calendar.NextLayoutUnitCommand.Execute(null);

        var changedMembers = new HashSet<string>();
        var days = calendar.Days();
        for (var cell = 0; cell < days.Count; cell++)
        {
            for (var m = 0; m < members.Length; m++)
            {
                var oldValue = before[cell][m];
                var newValue = members[m].GetValue(days[cell]);
                var changed = oldValue is IEnumerable oldItems && newValue is IEnumerable newItems
                    ? !oldItems.Cast<object>().SequenceEqual(newItems.Cast<object>())
                    : !Equals(oldValue, newValue);

                if (changed)
                {
                    changedMembers.Add(members[m].Name);
                    changes[cell].Should().Contain(members[m].Name,
                        $"cell {cell} changed {members[m].Name} from {oldValue} to {newValue}");
                }
            }
        }

        changedMembers.Should().Contain([
            nameof(ICalendarDay.Date),
            nameof(ICalendarDay.Day),
            nameof(ICalendarDay.IsSelected),
            nameof(ICalendarDay.IsThisMonth),
            nameof(ICalendarDay.IsDisabled),
            nameof(ICalendarDay.HasEvents),
            nameof(ICalendarDay.EventCount),
            nameof(ICalendarDay.Events),
            nameof(ICalendarDay.EventColors),
            nameof(ICalendarDay.IsRangeStart),
            nameof(ICalendarDay.IsRangeEnd),
        ], "the scenario must exercise these members");
    }
}
