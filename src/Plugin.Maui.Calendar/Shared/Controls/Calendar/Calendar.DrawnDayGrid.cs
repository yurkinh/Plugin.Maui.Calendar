using DrawnUi.Draw;
using DrawnUi.Views;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// The <see cref="AppContext"/> switch that draws the day grid with DrawnUI. Off by default while
	/// the drawn grid does not yet match the native one in every respect; it is read every time the
	/// layout is rendered.
	/// </summary>
	internal const string DrawnDayGridSwitch = "Plugin.Maui.Calendar.DrawnDayGrid";

	static bool IsDrawnDayGridEnabled => AppContext.TryGetSwitch(DrawnDayGridSwitch, out var enabled) && enabled;

	/// <summary>
	/// Whether the day grid should be drawn: the drawn grid shows the built-in cell only, so a
	/// <see cref="DayViewTemplate"/> keeps the native cells.
	/// </summary>
	bool UsesDrawnDayGrid => IsDrawnDayGridEnabled && DayViewTemplate is null;

	// Whether the current layout shows the drawn grid (set by RenderLayout).
	bool isDayGridDrawn;

	// Created the first time the drawn grid is shown and kept for the life of the calendar: across
	// layout changes, switches to the native grid and back, and handler changes (a Canvas that
	// gets a new handler draws its content again).
	Canvas drawnDayCanvas;
	DrawnDayGrid drawnDayGrid;

	/// <summary>The drawn grid, created on first use.</summary>
	void EnsureDrawnDayGrid()
	{
		if (drawnDayGrid is not null)
		{
			return;
		}

		// DrawnUI may report gestures on a thread of its own: the calendar handles them on the UI thread.
		drawnDayGrid = new DrawnDayGrid(this)
		{
			DayTapped = day => OnUiThread(() => DayView.Tap(day, this)),
		};

#if ANDROID
		// As with the native cells (see DayView): on Android the canvas takes the whole touch, so the
		// calendar's swipe recognizers never see a swipe that starts on a day and the grid hands it
		// over. Elsewhere the calendar's recognizers see it, and handing it over would count it twice.
		drawnDayGrid.Swiped = direction => OnUiThread(() => OnDrawnDaySwiped(direction));
#endif

#if ANDROID || IOS || MACCATALYST || WINDOWS
		// Sized by its content (the grid), as the native grid is.
		drawnDayCanvas = new Canvas
		{
			RenderingMode = RenderingModeType.Accelerated,

			// Shares a pan the grid does not keep (a vertical one) with a native ScrollView around the calendar.
			Gestures = GesturesMode.SoftLock,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Start,
			Content = drawnDayGrid,
		};
#else
		drawnDayCanvas = null;
#endif
	}

	/// <summary>
	/// A swipe the drawn grid recognized over the days (on Android, see EnsureDrawnDayGrid): handled
	/// like one of the calendar's swipe recognizers, unless swipe detection is off at this moment.
	/// </summary>
	internal void OnDrawnDaySwiped(SwipeDirection direction)
	{
		if (!SwipeDetectionDisabled)
		{
			OnSwiped(this, new SwipedEventArgs(null, direction));
		}
	}

	void OnUiThread(Action action)
	{
		if (Dispatcher.IsDispatchRequired)
		{
			Dispatcher.Dispatch(action);
		}
		else
		{
			action();
		}
	}

	/// <summary>
	/// The drawn grid while the calendar shows it, otherwise <see langword="null"/>. Without a
	/// platform (the net10.0 target, where nothing is rendered) the grid has no Canvas: a Canvas
	/// needs the platform's main thread as soon as it is created.
	/// </summary>
	internal DrawnDayGrid DrawnDayGrid => isDayGridDrawn ? drawnDayGrid : null;

	/// <summary>
	/// Puts the drawn grid in place of the native one, showing the day models of the new layout.
	/// Called by RenderLayout once the models hold real data.
	/// </summary>
	void ShowDrawnDayGrid()
	{
		drawnDayGrid.SetDays([.. dayViews.Select(dayView => (DayModel)dayView.BindingContext)]);

		if (drawnDayCanvas is not null)
		{
			daysControl.Add(drawnDayCanvas);
		}
	}
}
