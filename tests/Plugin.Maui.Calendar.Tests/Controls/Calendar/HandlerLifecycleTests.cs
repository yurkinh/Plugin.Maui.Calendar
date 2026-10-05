using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies what the calendar sets up when it gets a handler (it is about to be shown) and tears
/// down when the handler is removed: the swipe recognizers, day tap handling and observing Events.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class HandlerLifecycleTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static readonly SwipeDirection[] allDirections =
        [SwipeDirection.Left, SwipeDirection.Right, SwipeDirection.Up, SwipeDirection.Down];

    [Fact]
    public void Handler_AddsOneSwipeRecognizerPerDirectionAndRemovingItRemovesThem()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.SwipeGestures().Should().BeEmpty();

        calendar.AttachHandler();
        calendar.SwipeGestures().Select(gesture => gesture.Direction).Should().BeEquivalentTo(allDirections);

        calendar.Handler = null;
        calendar.SwipeGestures().Should().BeEmpty();
    }

    [Fact]
    public void SwipeDetectionDisabled_NoSwipeRecognizers()
    {
        var calendar = new TestCalendar { ShownDate = May15, SwipeDetectionDisabled = true };

        calendar.AttachHandler();

        calendar.SwipeGestures().Should().BeEmpty();
    }

    [Fact]
    public void SwipeDetectionDisabled_ChangedAtRuntime_AddsOrRemovesTheSwipeRecognizers()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.AttachHandler();

        calendar.SwipeDetectionDisabled = true;
        calendar.SwipeGestures().Should().BeEmpty();

        calendar.SwipeDetectionDisabled = false;
        calendar.SwipeGestures().Should().HaveCount(4);

        // Detaching after the change removes exactly the recognizers that are there.
        calendar.Handler = null;
        calendar.SwipeGestures().Should().BeEmpty();
    }

    [Fact]
    public void SwipeDetectionDisabled_ChangedWithoutHandler_AddsNothing()
    {
        var calendar = new TestCalendar { ShownDate = May15, SwipeDetectionDisabled = true };

        calendar.SwipeDetectionDisabled = false;

        calendar.SwipeGestures().Should().BeEmpty();
    }

    [Fact]
    public void Handler_RemovingIt_KeepsRecognizersAddedByTheApp()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var appRecognizer = new TapGestureRecognizer();
        calendar.GestureRecognizers.Add(appRecognizer);
        calendar.AttachHandler();

        calendar.Handler = null;

        calendar.GestureRecognizers.Should().Equal(appRecognizer);
    }

    [Fact]
    public void Handler_ReplacedByAnotherHandler_KeepsTapHandlingAndOneSetOfSwipeRecognizers()
    {
        // The new handler used to be attached before the old one was detached, and detaching then
        // unsubscribed from day taps and removed the recognizers that attaching had just added.
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.AttachHandler();

        calendar.AttachHandler(new FakeViewHandler());

        calendar.SwipeGestures().Should().HaveCount(4);
        calendar.CellFor(new DateTime(2025, 5, 20)).Tap();
        calendar.SelectedDate.Should().Be(new DateTime(2025, 5, 20));

        calendar.Handler = null;
    }

    [Fact]
    public void Handler_DayTapsAreOnlyHandledWhileTheCalendarHasAHandler()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.CellFor(new DateTime(2025, 5, 20)).Tap();
        calendar.SelectedDate.Should().BeNull();

        calendar.AttachHandler();
        calendar.CellFor(new DateTime(2025, 5, 21)).Tap();
        calendar.SelectedDate.Should().Be(new DateTime(2025, 5, 21));

        calendar.Handler = null;
        calendar.CellFor(new DateTime(2025, 5, 22)).Tap();
        calendar.SelectedDate.Should().Be(new DateTime(2025, 5, 21));
    }

    [Fact]
    public void Handler_EventsChangedWhileItWasRemoved_AreShownWhenItIsAttachedAgain()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.AttachHandler();
        calendar.Handler = null;

        calendar.Events.Add(new DateTime(2025, 5, 20), new List<string> { "Meeting" });
        calendar.DayFor(new DateTime(2025, 5, 20)).HasEvents.Should().BeFalse("events are not observed without a handler");

        calendar.AttachHandler();

        calendar.DayFor(new DateTime(2025, 5, 20)).HasEvents.Should().BeTrue();
        calendar.Handler = null;
    }

    [Fact]
    public void DayTappedMessage_WithoutSource_IsHandledByEveryCalendar()
    {
        var first = new TestCalendar { ShownDate = May15 };
        var second = new TestCalendar { ShownDate = May15 };
        first.AttachHandler();
        second.AttachHandler();
        try
        {
            WeakReferenceMessenger.Default.Send(new DayTappedMessage(new DateTime(2025, 5, 20)));

            first.SelectedDate.Should().Be(new DateTime(2025, 5, 20));
            second.SelectedDate.Should().Be(new DateTime(2025, 5, 20));
        }
        finally
        {
            first.Handler = null;
            second.Handler = null;
        }
    }
}
