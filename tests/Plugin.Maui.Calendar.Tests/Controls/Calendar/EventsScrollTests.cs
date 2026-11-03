using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies when the event list is scrolled to the top: a change of the selection scrolls it to
/// the top, a change of <c>Events</c> keeps its scroll position.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class EventsScrollTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static DateTime May(int day) => new(2025, 5, day);

    /// <summary>A calendar showing May 10 with two events, recording the scroll requests made after this setup.</summary>
    static (TestCalendar Calendar, List<double> ScrollRequests) CreateCalendarWithEvents()
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            SelectedDate = May(10),
            Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch" } },
        };
        var scrollRequests = new List<double>();
        ((IScrollViewController)calendar.Named<ScrollView>("eventsScrollView")).ScrollToRequested += (_, e) => scrollRequests.Add(e.ScrollY);
        return (calendar, scrollRequests);
    }

    [Fact]
    public void Events_ReplacedWithTheSameEvents_DoesNotScroll()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();

        // A refresh that produced the same events again, in a new collection.
        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch" } };

        calendar.SelectedDayEvents.Should().BeEquivalentTo(new[] { "Meeting", "Lunch" });
        scrollRequests.Should().BeEmpty();
    }

    [Fact]
    public void Events_ReplacedWithMoreEvents_ShowsThemWithoutScrolling()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();

        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch", "Dinner" } };

        calendar.SelectedDayEvents.Should().BeEquivalentTo(new[] { "Meeting", "Lunch", "Dinner" });
        scrollRequests.Should().BeEmpty();
    }

    [Fact]
    public void DayEvents_Added_ShowsThemWithoutScrolling()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        var dayEvents = new ObservableCollection<string> { "Meeting" };
        calendar.Events[May(10)] = dayEvents;

        dayEvents.Add("Lunch");

        calendar.SelectedDayEvents.Should().BeSameAs(dayEvents);
        scrollRequests.Should().BeEmpty();
    }

    [Fact]
    public void SelectedDate_Changed_ScrollsTheListToTheTop()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        calendar.Events[May(11)] = new List<string> { "Standup" };

        calendar.SelectedDate = May(11);

        calendar.SelectedDayEvents.Should().BeEquivalentTo(new[] { "Standup" });
        scrollRequests.Should().Equal(0);
    }

    [Fact]
    public void SelectedDate_SetToTheSameDate_DoesNotScroll()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();

        calendar.SelectedDate = May(10);

        scrollRequests.Should().BeEmpty();
    }

    [Fact]
    public void SelectedDates_SetToNull_ShowsNoEventsAndScrollsToTheTop()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();

        calendar.SelectedDates = null!;

        calendar.SelectedDayEvents.Should().BeNull();
        scrollRequests.Should().Contain(0);
    }
}
