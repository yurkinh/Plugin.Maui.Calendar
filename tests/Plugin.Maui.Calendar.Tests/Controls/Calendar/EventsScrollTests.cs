using System.Collections;
using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies when the event list is rebuilt and scrolled to the top: a change of the selection
/// scrolls to the top, a change of <c>Events</c> for the same selection keeps the list and its
/// scroll position.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class EventsScrollTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static DateTime May(int day) => new(2025, 5, day);

    /// <summary>A calendar showing May 10 with two events, recording the scroll requests made after this setup.</summary>
    static (TestCalendar Calendar, List<double> ScrollRequests) CreateCalendarWithEvents(bool keepScrollPosition = true)
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            SelectedDate = May(10),
            KeepEventsScrollPositionOnRefresh = keepScrollPosition,
            Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch" } },
        };
        var scrollRequests = new List<double>();
        calendar.EventsScrollRequested += (_, scrollY) => scrollRequests.Add(scrollY);
        return (calendar, scrollRequests);
    }

    [Fact]
    public void Events_ReplacedWithTheSameEvents_KeepsTheListAndDoesNotScroll()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        var shownEvents = calendar.SelectedDayEvents;

        // A refresh that produced the same events again, in a new collection.
        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch" } };

        calendar.SelectedDayEvents.Should().BeSameAs(shownEvents, "equal items must not recreate the rows");
        scrollRequests.Should().BeEmpty();
    }

    [Fact]
    public void Events_ReplacedWithMoreEvents_ShowsThemWithoutScrollingToTheTop()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();

        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch", "Dinner" } };

        calendar.SelectedDayEvents.Should().BeEquivalentTo(new[] { "Meeting", "Lunch", "Dinner" });
        scrollRequests.Should().NotContain(0);
    }

    [Fact]
    public void DayEvents_Added_KeepsTheListAndDoesNotScroll()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        var dayEvents = new ObservableCollection<string> { "Meeting" };
        calendar.Events[May(10)] = dayEvents;
        scrollRequests.Clear();

        dayEvents.Add("Lunch");

        calendar.SelectedDayEvents.Should().BeSameAs(dayEvents);
        scrollRequests.Should().BeEmpty();
    }

    [Fact]
    public void SelectedDate_Changed_ScrollsTheListToTheTop()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        calendar.Events[May(11)] = new List<string> { "Standup" };
        scrollRequests.Clear();

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
    public void KeepEventsScrollPositionOnRefresh_Off_ScrollsToTheTopOnEveryEventsChange()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents(keepScrollPosition: false);
        var shownEvents = calendar.SelectedDayEvents;

        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch" } };

        calendar.SelectedDayEvents.Should().NotBeSameAs(shownEvents);
        scrollRequests.Should().Equal(0);
    }

    /// <summary>
    /// Gives the event list the sizes a layout pass would: a <paramref name="viewportHeight"/> tall
    /// scroll view showing <paramref name="contentHeight"/> of rows.
    /// </summary>
    static (ScrollView ScrollView, VerticalStackLayout Content) LayOutEventsList(TestCalendar calendar, double viewportHeight, double contentHeight)
    {
        var scrollView = calendar.FindByName<ScrollView>("eventsScrollView");
        var content = calendar.FindByName<VerticalStackLayout>("eventsContent");
        scrollView.Frame = new Rect(0, 0, 400, viewportHeight);
        content.Frame = new Rect(0, 0, 400, contentHeight);
        return (scrollView, content);
    }

    [Fact]
    public void Events_ReplacedWithMoreEventsWhileScrolled_RestoresTheScrollPosition()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        var (scrollView, content) = LayOutEventsList(calendar, viewportHeight: 800, contentHeight: 1200);
        scrollView.SetScrolledPosition(0, 300);

        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch", "Dinner" } };
        // Restored right away (the test dispatcher runs inline) ...
        scrollRequests.Should().Equal(300);

        // ... and again when the recreated rows get their size.
        content.Frame = new Rect(0, 0, 400, 1500);
        scrollRequests.Should().Equal(300, 300);

        // Later size changes are not scroll restores.
        content.Frame = new Rect(0, 0, 400, 1600);
        scrollRequests.Should().HaveCount(2);
    }

    [Fact]
    public void Events_ReplacedWithFewerEventsWhileScrolled_StaysWithinTheShorterList()
    {
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        var (scrollView, content) = LayOutEventsList(calendar, viewportHeight: 800, contentHeight: 1200);
        scrollView.SetScrolledPosition(0, 300);

        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting" } };
        content.Frame = new Rect(0, 0, 400, 900);

        scrollRequests.Should().Equal(300, 100);
    }

    [Fact]
    public void Events_ReplacedWhileScrolled_RestoresOnceWhenTheRowsAreSizedBeforeTheDispatchedRestore()
    {
        var dispatcher = new QueueingDispatcher();
        using var dispatcherScope = SyncDispatcherProvider.Use(dispatcher);
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        var (scrollView, content) = LayOutEventsList(calendar, viewportHeight: 800, contentHeight: 1200);
        scrollView.SetScrolledPosition(0, 300);
        dispatcher.IsDispatchRequired = true;

        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch", "Dinner" } };
        content.Frame = new Rect(0, 0, 400, 1500);
        dispatcher.RunQueued();

        scrollRequests.Should().Equal(300);
    }

    [Fact]
    public void Dispose_CancelsAPendingScrollRestore()
    {
        var dispatcher = new QueueingDispatcher();
        using var dispatcherScope = SyncDispatcherProvider.Use(dispatcher);
        var (calendar, scrollRequests) = CreateCalendarWithEvents();
        var (scrollView, content) = LayOutEventsList(calendar, viewportHeight: 800, contentHeight: 1200);
        scrollView.SetScrolledPosition(0, 300);
        dispatcher.IsDispatchRequired = true;
        calendar.Events = new EventCollection { [May(10)] = new List<string> { "Meeting", "Lunch", "Dinner" } };

        calendar.Dispose();
        content.Frame = new Rect(0, 0, 400, 1500);
        dispatcher.RunQueued();

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

    [Fact]
    public void KeepEventsScrollPositionOnRefresh_DefaultsToTrue()
    {
        new CalendarControl().KeepEventsScrollPositionOnRefresh.Should().BeTrue();
    }

    public static TheoryData<ICollection?, ICollection?, bool> SameItemsCases => new()
    {
        { null, null, true },
        { null, new List<string>(), true },
        { new List<string> { "a" }, null, false },
        { new List<string> { "a", "b" }, new List<string> { "a", "b" }, true },
        { new List<string> { "a", "b" }, new List<string> { "b", "a" }, false },
        { new List<string> { "a", "b" }, new List<string> { "a", "b", "c" }, false },
        { new List<string> { "a", "b" }, new[] { "a", "b" }, true },
        // Equal by Equals, not by reference.
        { new List<object> { new DateTime(2025, 5, 10), 2 }, new List<object> { new DateTime(2025, 5, 10), 2 }, true },
        { new List<object> { new object() }, new List<object> { new object() }, false },
    };

    [Theory]
    [MemberData(nameof(SameItemsCases))]
    public void HaveSameItems_ComparesCountOrderAndEquality(ICollection? current, ICollection? updated, bool expected)
    {
        CalendarControl.HaveSameItems(current!, updated!).Should().Be(expected);
    }

    [Theory]
    [InlineData(500, 1200, 800, 400)]   // past the end of a shorter list: the end
    [InlineData(100, 1200, 800, 100)]   // still within the list: unchanged
    [InlineData(500, 600, 800, 0)]      // the list fits the view: the top
    [InlineData(500, -1, -1, 0)]        // not laid out yet
    public void ClampEventsScroll_KeepsThePositionWithinTheList(double scrollY, double contentHeight, double viewportHeight, double expected)
    {
        CalendarControl.ClampEventsScroll(scrollY, contentHeight, viewportHeight).Should().Be(expected);
    }
}
