using FluentAssertions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies the calendar's own swipe recognizers: they exist while the calendar has a handler,
/// follow <c>SwipeDetectionDisabled</c> at any time, and recognizers the app added are left alone.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class SwipeGestureTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static List<SwipeDirection> SwipeDirections(TestCalendar calendar) =>
        [.. calendar.GestureRecognizers.OfType<SwipeGestureRecognizer>().Select(swipe => swipe.Direction)];

    [Fact]
    public void SwipeRecognizers_AddedWithTheHandlerAndRemovedWithIt()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        SwipeDirections(calendar).Should().BeEmpty();

        calendar.Handler = new FakeViewHandler();

        SwipeDirections(calendar).Should().BeEquivalentTo(
            [SwipeDirection.Left, SwipeDirection.Right, SwipeDirection.Up, SwipeDirection.Down]);

        calendar.Handler = null;

        SwipeDirections(calendar).Should().BeEmpty();
    }

    [Fact]
    public void SwipeDetectionDisabled_SwitchedWhileShown_RemovesAndAddsOnlyTheCalendarsRecognizers()
    {
        var appTap = new TapGestureRecognizer();
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.GestureRecognizers.Add(appTap);
        calendar.Handler = new FakeViewHandler();

        calendar.SwipeDetectionDisabled = true;

        calendar.GestureRecognizers.Should().Equal(appTap);

        calendar.SwipeDetectionDisabled = false;

        SwipeDirections(calendar).Should().HaveCount(4);
        calendar.GestureRecognizers.Should().Contain(appTap);

        calendar.Handler = null;

        calendar.GestureRecognizers.Should().Equal(appTap);
    }

    [Fact]
    public void SwipeDetectionEnabledWhileShown_HandlerRemovedAfterwards_DoesNotThrow()
    {
        // This order used to throw a NullReferenceException on detach: swipes disabled when the
        // handler attached, a recognizer of the app on the calendar, then swipes enabled.
        var appTap = new TapGestureRecognizer();
        var calendar = new TestCalendar { ShownDate = May15, SwipeDetectionDisabled = true };
        calendar.GestureRecognizers.Add(appTap);
        calendar.Handler = new FakeViewHandler();
        calendar.SwipeDetectionDisabled = false;

        Action detach = () => calendar.Handler = null;

        detach.Should().NotThrow();
        calendar.GestureRecognizers.Should().Equal(appTap);
    }

    [Fact]
    public void SwipeDetectionChangedWithoutHandler_AppliedWhenTheCalendarIsShownAgain()
    {
        var calendar = new TestCalendar { ShownDate = May15, SwipeDetectionDisabled = true };
        calendar.Handler = new FakeViewHandler();
        calendar.Handler = null;

        calendar.SwipeDetectionDisabled = false;

        SwipeDirections(calendar).Should().BeEmpty("the recognizers exist only while the calendar has a handler");

        calendar.Handler = new FakeViewHandler();

        SwipeDirections(calendar).Should().HaveCount(4);
        calendar.Handler = null;
    }

    [Fact]
    public void HandlerReplacedByAnother_KeepsSwipeRecognizersAndDayTaps()
    {
        var date = new DateTime(2025, 5, 12);
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.EnsureAllContent();
        calendar.Handler = new FakeViewHandler();

        calendar.Handler = new FakeViewHandler();

        SwipeDirections(calendar).Should().HaveCount(4, "the old handler's recognizers are replaced, not added to");
        calendar.CellFor(date).Tap();
        calendar.SelectedDate.Should().Be(date, "the calendar still listens for day taps");
        calendar.Handler = null;
    }
}
