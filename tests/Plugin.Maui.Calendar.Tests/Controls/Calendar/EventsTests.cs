using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies how the calendar shows its <c>Events</c>: the event list of the selected dates, the
/// indicator colors of each day and the notifications of the visible date range.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class EventsTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static DateTime May(int day) => new(2025, 5, day);

    // ── Events ───────────────────────────────────────────────────────────────

    [Fact]
    public void Events_SetToNull_ShowsNoEventsInsteadOfThrowing()
    {
        // A binding to a view model whose collection is not created yet sets null.
        var calendar = new TestCalendar { ShownDate = May15, SelectedDate = May(10) };
        calendar.Events.Add(May(10), new List<string> { "Meeting" });

        calendar.Events = null!;

        calendar.Events.Should().BeEmpty();
        calendar.DayFor(May(10)).HasEvents.Should().BeFalse();
        calendar.SelectedDayEvents.Should().BeNull();

        // The empty collection is observed like any other.
        calendar.Events.Add(May(11), new List<string> { "Lunch" });
        calendar.DayFor(May(11)).HasEvents.Should().BeTrue();
    }

    [Fact]
    public void Events_Replaced_StopsObservingTheOldCollection()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var oldEvents = calendar.Events;

        calendar.Events = new EventCollection();
        oldEvents.Add(May(10), new List<string> { "Meeting" });

        calendar.DayFor(May(10)).HasEvents.Should().BeFalse();
    }

    [Fact]
    public void Events_NullEntryForADay_CountsAsADayWithoutItems()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.Events.Add(May(10), null!);

        var day = calendar.DayFor(May(10));
        day.HasEvents.Should().BeTrue();
        day.EventCount.Should().Be(0);
        day.Events.Should().BeEmpty();
        day.EventColors.Should().Equal(calendar.EventIndicatorColor);
    }

    [Fact]
    public void SelectedDayEvents_AreTheEventsOfTheSelectedDay()
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch" } },
        };

        calendar.SelectedDate = May(10);
        calendar.SelectedDayEvents.Cast<object>().Should().Equal("Meeting", "Lunch");

        calendar.SelectedDate = May(11);
        calendar.SelectedDayEvents.Should().BeNull();
    }

    // ── Indicator colors of a day ────────────────────────────────────────────

    [Fact]
    public void PersonalizedEvents_UseTheirOwnIndicatorColors()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.Events.Add(May(10), new PersonalizedEvents
        {
            EventIndicatorColor = Colors.Red,
            EventIndicatorSelectedColor = Colors.Green,
            EventIndicatorTextColor = Colors.Blue,
            EventIndicatorSelectedTextColor = Colors.Yellow,
        });

        var day = calendar.DayFor(May(10));

        day.EventIndicatorColor.Should().Be(Colors.Red);
        day.EventIndicatorSelectedColor.Should().Be(Colors.Green);
        day.EventIndicatorTextColor.Should().Be(Colors.Blue);
        day.EventIndicatorSelectedTextColor.Should().Be(Colors.Yellow);
        day.EventColors.Should().Equal(Colors.Red);

        calendar.SelectedDate = May(10);
        day.EventColors.Should().Equal(Colors.Green);
    }

    [Fact]
    public void PersonalizedEvents_WithoutSelectedColors_UseTheirUnselectedColors()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.Events.Add(May(10), new PersonalizedEvents { EventIndicatorColor = Colors.Red, EventIndicatorTextColor = Colors.Blue });

        var day = calendar.DayFor(May(10));

        day.EventIndicatorSelectedColor.Should().Be(Colors.Red);
        day.EventIndicatorSelectedTextColor.Should().Be(Colors.Blue);
    }

    [Fact]
    public void PersonalizedEvents_WithoutColors_UseTheCalendarsColors()
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            EventIndicatorColor = Colors.Orange,
            EventIndicatorSelectedColor = Colors.Purple,
            EventIndicatorTextColor = Colors.Brown,
            EventIndicatorSelectedTextColor = Colors.Pink,
        };
        calendar.Events.Add(May(10), new PersonalizedEvents());

        var day = calendar.DayFor(May(10));

        day.EventIndicatorColor.Should().Be(Colors.Orange);
        day.EventIndicatorSelectedColor.Should().Be(Colors.Purple);
        day.EventIndicatorTextColor.Should().Be(Colors.Brown);
        day.EventIndicatorSelectedTextColor.Should().Be(Colors.Pink);
    }

    [Fact]
    public void MultiColorEvents_ShowUpToFiveColors()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        Color[] colors = [Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Orange, Colors.Purple];

        calendar.Events.Add(May(10), new MultiColorEvents(colors));

        calendar.DayFor(May(10)).EventColors.Should().Equal(colors.Take(5));
    }

    [Fact]
    public void IndicatorColorsChangedAtRuntime_UpdateTheDays()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.Events.Add(May(10), new List<string> { "Meeting" });
        var day = calendar.DayFor(May(10));

        calendar.EventIndicatorColor = Colors.Red;
        calendar.EventIndicatorSelectedColor = Colors.Green;
        calendar.EventIndicatorTextColor = Colors.Blue;
        calendar.EventIndicatorSelectedTextColor = Colors.Yellow;

        day.EventColors.Should().Equal(Colors.Red);
        day.EventIndicatorSelectedColor.Should().Be(Colors.Green);
        day.EventIndicatorTextColor.Should().Be(Colors.Blue);
        day.EventIndicatorSelectedTextColor.Should().Be(Colors.Yellow);
    }

    // ── Visible date range ───────────────────────────────────────────────────

    [Fact]
    public void ShownDatesChanged_RaisedWithTheNewVisibleRangeAndExecutesItsCommandWhenItCanExecute()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var raised = new List<(DateTime, DateTime)>();
        var executed = new List<ShownDatesChangedEventArgs>();
        var canExecute = true;
        calendar.ShownDatesChanged += (_, args) => raised.Add((args.VisibleStartDate, args.VisibleEndDate));
        calendar.ShownDatesChangedCommand = new Command<ShownDatesChangedEventArgs>(executed.Add, args => canExecute && args is not null);

        calendar.ShownDate = new DateTime(2025, 6, 10);
        canExecute = false;
        calendar.ShownDate = new DateTime(2025, 7, 10);

        raised.Should().Equal(
            (new DateTime(2025, 6, 1), new DateTime(2025, 7, 12)),
            (new DateTime(2025, 6, 29), new DateTime(2025, 8, 9)));
        executed.Should().ContainSingle()
            .Which.VisibleStartDate.Should().Be(new DateTime(2025, 6, 1));
        calendar.VisibleStartDate.Should().Be(new DateTime(2025, 6, 29));
        calendar.VisibleEndDate.Should().Be(new DateTime(2025, 8, 9));
    }

    [Fact]
    public void ShownDatesChanged_NotRaisedWhenTheVisibleRangeStaysTheSame()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var raised = 0;
        calendar.ShownDatesChanged += (_, _) => raised++;

        calendar.ShownDate = May(20);
        calendar.SelectedDate = May(3);

        raised.Should().Be(0);
    }

    [Fact]
    public void ShownDatesChanged_WithoutSubscribersOrCommand_StillUpdatesTheVisibleRange()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.ShownDate = new DateTime(2025, 6, 10);

        calendar.VisibleStartDate.Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    public void ShownDatesChanged_OnlyTheEndOfTheVisibleRangeChanged_IsRaised()
    {
        // A week layout and a two-week layout that start on the same day only differ in their end.
        var calendar = new TestCalendar { CalendarLayout = WeekLayout.Week, ShownDate = May15 };
        var raised = new List<DateTime>();
        calendar.ShownDatesChanged += (_, args) => raised.Add(args.VisibleEndDate);

        calendar.CalendarLayout = WeekLayout.TwoWeek;

        raised.Should().Equal(May(24));
    }
}
