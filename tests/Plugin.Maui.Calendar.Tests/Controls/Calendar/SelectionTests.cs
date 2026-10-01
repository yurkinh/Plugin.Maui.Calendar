using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies selecting dates through the calendar: <c>SelectedDate</c>, <c>SelectedDates</c>,
/// <c>ClearSelection</c>, <c>AllowDeselecting</c>, dates with a time, and the selection calendars
/// (<c>MultiSelectionCalendar</c>, <c>RangeSelectionCalendar</c>, <c>WeekSelectionCalendar</c>).
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class SelectionTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static DateTime May(int day, int hour = 0) => new(2025, 5, day, hour, 0, 0);

    static List<DateTime> SelectedCells(IDayCells calendar) =>
        [.. calendar.Days().Where(day => day.IsSelected).Select(day => day.Date)];

    // ── SelectedDate and SelectedDates ───────────────────────────────────────

    [Fact]
    public void SelectedDate_SetToNull_ClearsSelectedDates()
    {
        var calendar = new TestCalendar { ShownDate = May15, SelectedDate = May(10) };

        calendar.SelectedDate = null;

        calendar.SelectedDates.Should().BeEmpty();
        SelectedCells(calendar).Should().BeEmpty();
    }

    [Fact]
    public void SelectedDates_SetToNull_ClearsTheSelection()
    {
        var calendar = new TestCalendar { ShownDate = May15, SelectedDate = May(10) };

        calendar.SelectedDates = null!;

        calendar.SelectedDate.Should().BeNull();
        SelectedCells(calendar).Should().BeEmpty();

        // A new collection after null is observed again.
        calendar.SelectedDates = [May(12)];
        SelectedCells(calendar).Should().Equal(May(12));
    }

    [Fact]
    public void SelectedDates_ItemsAddedOrRemoved_UpdateTheSelection()
    {
        var calendar = new TestMultiSelectionCalendar { ShownDate = May15 };
        var dates = new ObservableCollection<DateTime> { May(3) };
        calendar.SelectedDates = dates;

        dates.Add(May(7));
        SelectedCells(calendar).Should().Equal(May(3), May(7));

        dates.Remove(May(3));
        SelectedCells(calendar).Should().Equal(May(7));
        calendar.SelectedDateText.Should().Be("7 May 2025");
    }

    [Fact]
    public void SelectedDates_ReplacedCollection_IsNoLongerObserved()
    {
        var calendar = new TestMultiSelectionCalendar { ShownDate = May15 };
        var oldDates = new ObservableCollection<DateTime>();
        calendar.SelectedDates = oldDates;
        calendar.SelectedDates = [May(5)];

        oldDates.Add(May(9));

        SelectedCells(calendar).Should().Equal(May(5));
    }

    [Fact]
    public void SelectedDate_WithATime_SelectsItsDayAndATapOnItDeselectsIt()
    {
        var calendar = new TestCalendar { ShownDate = May15, SelectedDate = May(10, 15) };
        SelectedCells(calendar).Should().Equal(May(10));

        calendar.WithTapHandling(() => calendar.CellFor(May(10)).Tap());

        calendar.SelectedDate.Should().BeNull();
    }

    [Fact]
    public void MultiSelection_DatesWithATime_SelectTheirDays()
    {
        var calendar = new TestMultiSelectionCalendar { ShownDate = May15, SelectedDates = [May(2, 9), May(4, 18)] };

        SelectedCells(calendar).Should().Equal(May(2), May(4));
    }

    [Fact]
    public void DisabledDates_WithATime_DisableTheirDaysAndCannotBeSelected()
    {
        var calendar = new TestCalendar { ShownDate = May15, DisabledDates = [May(20, 14)] };

        calendar.DayFor(May(20)).IsDisabled.Should().BeTrue();

        calendar.WithTapHandling(() => calendar.CellFor(May(20)).Tap());
        calendar.SelectedDate.Should().BeNull();
    }

    [Fact]
    public void ClearSelection_ClearsSelectedDateAndSelectedDates()
    {
        var calendar = new TestMultiSelectionCalendar { ShownDate = May15, SelectedDates = [May(2), May(4)] };

        calendar.ClearSelection();

        calendar.SelectedDate.Should().BeNull();
        calendar.SelectedDates.Should().BeNull();
        SelectedCells(calendar).Should().BeEmpty();
    }

    [Fact]
    public void AllowDeselecting_Off_TapOnTheSelectedDayKeepsIt()
    {
        var calendar = new TestCalendar { ShownDate = May15, AllowDeselecting = false, SelectedDate = May(10) };

        calendar.WithTapHandling(() => calendar.CellFor(May(10)).Tap());

        calendar.SelectedDate.Should().Be(May(10));
        calendar.DayFor(May(10)).AllowDeselect.Should().BeFalse();
    }

    [Fact]
    public void SelectionEngine_ReplacedByARangeEngine_SelectsTheRangeOnAPlainCalendar()
    {
        // CurrentSelectionEngine is public; a range engine on a plain Calendar has no range colors to apply.
        var calendar = new TestCalendar { ShownDate = May15, CurrentSelectionEngine = new RangedSelectionEngine() };

        calendar.SelectedDates = [May(3), May(6)];

        SelectedCells(calendar).Should().Equal(May(3), May(4), May(5), May(6));
        calendar.ShownDate = new DateTime(2025, 6, 1);
    }

    // ── RangeSelectionCalendar ───────────────────────────────────────────────

    [Fact]
    public void Range_SelectedStartAndEndDateFromCode_SelectTheRange()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15 };

        calendar.SelectedStartDate = May(10);
        calendar.SelectedEndDate = May(13);

        SelectedCells(calendar).Should().Equal(May(10), May(11), May(12), May(13));
        calendar.SelectedDates.Should().Equal(May(10), May(11), May(12), May(13));
    }

    [Fact]
    public void Range_SelectedDatesFromCode_SelectTheWholeRangeBetweenTheEarliestAndLatestDate()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15 };

        calendar.SelectedDates = [May(13), May(10)];

        calendar.SelectedDates.Should().Equal(May(10), May(11), May(12), May(13));
        calendar.SelectedStartDate.Should().Be(May(10));
        calendar.SelectedEndDate.Should().Be(May(13));
    }

    [Fact]
    public void Range_SelectedEndDateFromCode_LeavesDisabledDaysOutOfSelectedDates()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15, DisabledDates = [May(11)] };

        calendar.SelectedStartDate = May(10);
        calendar.SelectedEndDate = May(13);

        calendar.SelectedDates.Should().Equal(May(10), May(12), May(13));
    }

    [Fact]
    public void Range_SelectedStartDateSetToNull_ClearsTheRange()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15 };
        calendar.SelectedStartDate = May(10);
        calendar.SelectedEndDate = May(13);

        calendar.SelectedStartDate = null;

        calendar.SelectedDates.Should().BeEmpty();
        SelectedCells(calendar).Should().BeEmpty();
    }

    [Fact]
    public void Range_TapsSelectTheRangeAndSetItsBorders()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15 };

        calendar.WithTapHandling(() =>
        {
            calendar.CellFor(May(10)).Tap();
            calendar.CellFor(May(12)).Tap();
        });

        calendar.SelectedStartDate.Should().Be(May(10));
        calendar.SelectedEndDate.Should().Be(May(12));
        SelectedCells(calendar).Should().Equal(May(10), May(11), May(12));
        calendar.SelectedDateText.Should().Be("10 May 2025 - 12 May 2025");
    }

    [Fact]
    public void Range_BackgroundColor_DefaultsToTheSelectedDayColorAndPaintsTheDaysInside()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15, SelectedDayBackgroundColor = Colors.Blue };
        calendar.SelectedDatesRangeBackgroundColor.Should().Be(Colors.Blue);

        calendar.SelectedDatesRangeBackgroundColor = Colors.LightBlue;
        calendar.SelectedStartDate = May(10);
        calendar.SelectedEndDate = May(12);

        calendar.DayFor(May(10)).SelectedBackgroundColor.Should().Be(Colors.Blue);
        calendar.DayFor(May(11)).SelectedBackgroundColor.Should().Be(Colors.LightBlue);
        calendar.DayFor(May(12)).SelectedBackgroundColor.Should().Be(Colors.Blue);
    }

    [Fact]
    public void Range_ShownDateChanged_KeepsTheSelectedRange()
    {
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15 };
        calendar.SelectedStartDate = May(10);
        calendar.SelectedEndDate = May(12);
        var selectedDates = calendar.SelectedDates;

        calendar.ShownDate = new DateTime(2025, 6, 1);
        calendar.ShownDate = May15;

        calendar.SelectedDates.Should().BeSameAs(selectedDates, "an unchanged range is not assigned again");
        SelectedCells(calendar).Should().Equal(May(10), May(11), May(12));
    }

    // ── WeekSelectionCalendar ────────────────────────────────────────────────

    [Fact]
    public void Week_TapSelectsItsWeekFromFirstDayOfWeek()
    {
        var calendar = new TestWeekSelectionCalendar { ShownDate = May15, FirstDayOfWeek = DayOfWeek.Monday };

        calendar.WithTapHandling(() => calendar.CellFor(May(15)).Tap());

        SelectedCells(calendar).Should().Equal(Enumerable.Range(12, 7).Select(day => May(day)));
    }
}
