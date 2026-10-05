using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies that the calendar takes today from its <c>TimeProvider</c>: the highlighted day, its
/// change at midnight and the wait of the today-refresh timer.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class TimeProviderTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static FakeTimeProvider At(DateTime localTime) => new(new DateTimeOffset(localTime, TimeSpan.Zero));

    static List<DateTime> Todays(TestCalendar calendar) =>
        [.. calendar.Days().Where(day => day.IsToday).Select(day => day.Date)];

    [Fact]
    public void TimeProvider_DefaultsToTheSystemClock()
    {
        new TestCalendar().TimeProvider.Should().BeSameAs(TimeProvider.System);
    }

    [Fact]
    public void TimeProvider_PinsToday()
    {
        var calendar = new TestCalendar { TimeProvider = At(new DateTime(2025, 5, 20, 9, 30, 0)), ShownDate = May15 };

        Todays(calendar).Should().Equal(new DateTime(2025, 5, 20));
    }

    [Fact]
    public void TimeProvider_ChangedAtRuntime_MovesTheTodayHighlight()
    {
        var calendar = new TestCalendar { TimeProvider = At(new DateTime(2025, 5, 20)), ShownDate = May15 };

        calendar.TimeProvider = At(new DateTime(2025, 5, 3));

        Todays(calendar).Should().Equal(new DateTime(2025, 5, 3));
    }

    [Fact]
    public void TimeProvider_SetToNull_FallsBackToTheSystemClock()
    {
        var calendar = new TestCalendar { TimeProvider = At(new DateTime(2025, 5, 20)) };

        calendar.TimeProvider = null!;

        calendar.TimeProvider.Should().BeSameAs(TimeProvider.System);
    }

    [Fact]
    public void TimeProvider_ShownMonthWithoutToday_HighlightsNoDay()
    {
        var calendar = new TestCalendar { TimeProvider = At(new DateTime(2030, 1, 1)), ShownDate = May15 };

        Todays(calendar).Should().BeEmpty();
    }

    [Fact]
    public void TodayRefreshTimer_WaitsUntilTheProvidersNextMidnightAndMovesTodayOnTick()
    {
        var time = At(new DateTime(2025, 5, 20, 23, 30, 0));
        var calendar = new TestCalendar { TimeProvider = time, ShownDate = May15 };

        calendar.SimulateLoaded();
        var timer = SyncDispatcher.Instance.CreatedTimers[^1];
        try
        {
            timer.Interval.Should().Be(TimeSpan.FromMinutes(30) + TimeSpan.FromSeconds(1));

            time.Advance(TimeSpan.FromMinutes(31));
            timer.Fire();

            Todays(calendar).Should().Equal(new DateTime(2025, 5, 21));
            timer.Interval.Should().Be(TimeSpan.FromHours(1), "the next midnight is almost a day away, so the wait is capped");
        }
        finally
        {
            calendar.SimulateUnloaded();
        }
    }

    [Fact]
    public void TimeProvider_ChangedWhileLoaded_RestartsTheTimerWithTheNewProvidersTime()
    {
        var calendar = new TestCalendar { TimeProvider = At(new DateTime(2025, 5, 20, 12, 0, 0)), ShownDate = May15 };

        calendar.SimulateLoaded();
        var timer = SyncDispatcher.Instance.CreatedTimers[^1];
        try
        {
            calendar.TimeProvider = At(new DateTime(2025, 5, 20, 23, 59, 0));

            timer.IsRunning.Should().BeTrue();
            timer.Interval.Should().Be(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        }
        finally
        {
            calendar.SimulateUnloaded();
        }
    }

    [Fact]
    public void Loaded_InAWindow_RefreshesTodayWhenTheAppResumes()
    {
        var time = At(new DateTime(2025, 5, 20, 12, 0, 0));
        var calendar = new TestCalendar { TimeProvider = time, ShownDate = May15 };
        var window = new Window(new ContentPage { Content = calendar });

        calendar.SimulateLoaded();
        try
        {
            // The device slept past midnight: the platform timer did not tick.
            time.Advance(TimeSpan.FromDays(1));
            Todays(calendar).Should().Equal(new DateTime(2025, 5, 20));

            ((IWindow)window).Resumed();

            Todays(calendar).Should().Equal(new DateTime(2025, 5, 21));
        }
        finally
        {
            calendar.SimulateUnloaded();
        }

        // Unloaded stops observing the window.
        time.Advance(TimeSpan.FromDays(1));
        ((IWindow)window).Resumed();
        Todays(calendar).Should().Equal(new DateTime(2025, 5, 21));
    }

    [Fact]
    public void Loaded_WithoutADispatcher_SkipsTheTimer()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var timersBefore = SyncDispatcher.Instance.CreatedTimers.Count;

        DispatcherProvider.SetCurrent(new NoDispatcherProvider());
        try
        {
            calendar.SimulateLoaded();
        }
        finally
        {
            DispatcherProvider.SetCurrent(new SyncDispatcherProvider());
            calendar.SimulateUnloaded();
        }

        SyncDispatcher.Instance.CreatedTimers.Should().HaveCount(timersBefore);
    }

    sealed class NoDispatcherProvider : Microsoft.Maui.Dispatching.IDispatcherProvider
    {
        public Microsoft.Maui.Dispatching.IDispatcher? GetForCurrentThread() => null;
    }

    [Fact]
    public void Dispose_StopsTheTimer()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.SimulateLoaded();
        var timer = SyncDispatcher.Instance.CreatedTimers[^1];

        calendar.Dispose();

        timer.IsRunning.Should().BeFalse();
        timer.TickSubscriberCount.Should().Be(0);
    }

    [Fact]
    public void DisposeFalse_LeavesTheTimerAlone()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.SimulateLoaded();
        var timer = SyncDispatcher.Instance.CreatedTimers[^1];
        try
        {
            // What a finalizer of a derived class would call: managed objects are not touched.
            calendar.CallDispose(false);

            timer.IsRunning.Should().BeTrue();
        }
        finally
        {
            calendar.SimulateUnloaded();
        }
    }

    [Fact]
    public void GetTodayRefreshInterval_IsUsedByTheTimer() =>
        CalendarControl.GetTodayRefreshInterval(new DateTime(2025, 5, 20, 23, 0, 0))
            .Should().Be(TimeSpan.FromHours(1));
}
