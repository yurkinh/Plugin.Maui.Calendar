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
