using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	// Upper bound for a single wait of the today-refresh timer. The timer normally fires just
	// after local midnight; capping the wait also corrects for time-zone or daylight-saving
	// changes and for platform timers that stop counting while the device sleeps.
	static readonly TimeSpan maxTodayRefreshInterval = TimeSpan.FromHours(1);

	// Fire slightly after midnight so a tick that arrives a little early still sees the new day.
	static readonly TimeSpan todayRefreshMidnightPadding = TimeSpan.FromSeconds(1);

	// Repeating timer that re-evaluates IsToday on every day cell after midnight; each tick sets the
	// wait until the next one. It only exists while the calendar is loaded, so an unloaded or
	// disposed calendar is never kept alive by it.
	IDispatcherTimer todayRefreshTimer;

	// Window whose Resumed event is observed while loaded: platform timers may be paused while
	// the app is in the background, so the today state is also refreshed when the app resumes.
	Window todayRefreshWindow;

	void OnCalendarLoaded(object sender, EventArgs e)
	{
		// The calendar may have been off screen across midnight.
		RefreshToday();
		StartTodayRefresh();
	}

	void OnCalendarUnloaded(object sender, EventArgs e) => StopTodayRefresh();

	void StartTodayRefresh()
	{
		StopTodayRefresh();

		if (Window is Window window)
		{
			todayRefreshWindow = window;
			todayRefreshWindow.Resumed += OnTodayRefreshWindowResumed;
		}

		// Loaded is raised on the UI thread, so the current thread's dispatcher is the right one.
		// Unlike the Dispatcher property, GetForCurrentThread returns null instead of throwing
		// when there is none (e.g. in unit tests); the timer is then skipped and UpdateDays
		// still re-evaluates IsToday on every render.
		var dispatcher = Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread();
		if (dispatcher is null)
		{
			return;
		}

		// Repeating on purpose: MAUI stops a one-shot timer right after its Tick handlers return, so
		// a one-shot timer restarted from inside Tick would be stopped again and never fire twice.
		// A repeating timer reads Interval again when it re-arms after each tick.
		todayRefreshTimer = dispatcher.CreateTimer();
		todayRefreshTimer.IsRepeating = true;
		todayRefreshTimer.Tick += OnTodayRefreshTimerTick;
		ScheduleTodayRefresh();
	}

	void StopTodayRefresh()
	{
		if (todayRefreshTimer is not null)
		{
			todayRefreshTimer.Stop();
			todayRefreshTimer.Tick -= OnTodayRefreshTimerTick;
			todayRefreshTimer = null;
		}

		if (todayRefreshWindow is not null)
		{
			todayRefreshWindow.Resumed -= OnTodayRefreshWindowResumed;
			todayRefreshWindow = null;
		}
	}

	// Restarts the wait from now. Only called outside Tick (see StartTodayRefresh).
	void ScheduleTodayRefresh()
	{
		if (todayRefreshTimer is null)
		{
			return;
		}

		todayRefreshTimer.Stop();
		todayRefreshTimer.Interval = GetTodayRefreshInterval(DateTime.Now);
		todayRefreshTimer.Start();
	}

	void OnTodayRefreshTimerTick(object sender, EventArgs e)
	{
		RefreshToday();

		// The timer keeps running and waits this long before the next tick; stopping and starting
		// it here would not work (see StartTodayRefresh).
		if (todayRefreshTimer is not null)
		{
			todayRefreshTimer.Interval = GetTodayRefreshInterval(DateTime.Now);
		}
	}

	void OnTodayRefreshWindowResumed(object sender, EventArgs e)
	{
		RefreshToday();
		ScheduleTodayRefresh();
	}

	/// <summary>
	/// Re-evaluates <see cref="DayModel.IsToday"/> on every day cell against the current local
	/// date. Cells whose value changes raise PropertyChanged for it and for the colors that
	/// depend on it; the others are left untouched.
	/// </summary>
	internal void RefreshToday()
	{
		var today = DateTime.Today;
		var changed = false;

		foreach (var dayView in dayViews)
		{
			if (dayView.BindingContext is DayModel dayModel)
			{
				changed |= dayModel.RefreshIsToday(today);
			}
		}

		// A DataTemplateSelector may choose by IsToday.
		if (changed)
		{
			RefreshDayTemplateSelection();
		}
	}

	/// <summary>
	/// Returns how long to wait from <paramref name="now"/> until the next today-refresh: just
	/// past the next local midnight, but never longer than <see cref="maxTodayRefreshInterval"/>.
	/// </summary>
	internal static TimeSpan GetTodayRefreshInterval(DateTime now)
	{
		var untilNextDay = now.Date.AddDays(1) - now + todayRefreshMidnightPadding;
		return untilNextDay < maxTodayRefreshInterval ? untilNextDay : maxTodayRefreshInterval;
	}
}
