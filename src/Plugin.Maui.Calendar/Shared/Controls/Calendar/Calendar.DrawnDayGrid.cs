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

	/// <summary>
	/// The <see cref="AppContext"/> switch that draws the day grid with Microsoft.Maui.Graphics instead
	/// (the proof of concept of the migration away from DrawnUI). It is read when a calendar creates
	/// its drawn grid, and wins over <see cref="DrawnDayGridSwitch"/>.
	/// </summary>
	internal const string GraphicsDayGridSwitch = "Plugin.Maui.Calendar.GraphicsDayGrid";

	static bool IsGraphicsDayGridEnabled => AppContext.TryGetSwitch(GraphicsDayGridSwitch, out var enabled) && enabled;

	static bool IsDrawnDayGridEnabled =>
		IsGraphicsDayGridEnabled || (AppContext.TryGetSwitch(DrawnDayGridSwitch, out var enabled) && enabled);

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
	View drawnDayCanvas;
	IDrawnDayGrid drawnDayGrid;

	/// <summary>The drawn grid, created on first use.</summary>
	void EnsureDrawnDayGrid()
	{
		if (drawnDayGrid is not null)
		{
			return;
		}

		// The Microsoft.Maui.Graphics grid is a view of its own: it needs no canvas around it.
		if (IsGraphicsDayGridEnabled)
		{
			var graphicsDayGrid = new GraphicsDayGrid(this) { DayTapped = day => DayView.Tap(day, this) };
#if ANDROID
			graphicsDayGrid.Swiped = OnDrawnDaySwiped;
#endif
			drawnDayGrid = graphicsDayGrid;
			drawnDayCanvas = graphicsDayGrid;
			return;
		}

		// DrawnUI may report gestures on a thread of its own: the calendar handles them on the UI thread.
		var grid = new DrawnDayGrid(this)
		{
			DayTapped = day => OnUiThread(() => DayView.Tap(day, this)),
		};
		drawnDayGrid = grid;

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
			Content = grid,
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

	// The native grid is mirrored by the platform for a right-to-left FlowDirection (the calendar's own
	// or an inherited one); the drawn grid mirrors its columns itself.
	bool IsRightToLeft => (((IVisualElementController)this).EffectiveFlowDirection & EffectiveFlowDirection.RightToLeft) != 0;

	protected override void OnPropertyChanged(string propertyName = null)
	{
		base.OnPropertyChanged(propertyName);

		if (propertyName == nameof(FlowDirection) && drawnDayGrid is not null)
		{
			drawnDayGrid.IsRightToLeft = IsRightToLeft;
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
	/// The drawn grid while the calendar shows it, otherwise <see langword="null"/>; created on first
	/// use. Without a platform (the net10.0 target, where nothing is rendered) the grid has no Canvas:
	/// a Canvas needs the platform's main thread as soon as it is created.
	/// </summary>
	internal DrawnDayGrid DrawnDayGrid => ShownDrawnDayGrid as DrawnDayGrid;

	/// <summary>
	/// The grid drawn with Microsoft.Maui.Graphics while the calendar shows it (see
	/// <see cref="GraphicsDayGridSwitch"/>), otherwise <see langword="null"/>; created on first use.
	/// </summary>
	internal GraphicsDayGrid GraphicsDayGrid => ShownDrawnDayGrid as GraphicsDayGrid;

	IDrawnDayGrid ShownDrawnDayGrid
	{
		get
		{
			if (isDayGridDrawn && drawnDayGrid is null)
			{
				ShowDrawnDayGrid();
			}

			return isDayGridDrawn ? drawnDayGrid : null;
		}
	}

	/// <summary>Whether the drawn grid was ever created.</summary>
	internal bool IsDrawnDayGridCreated => drawnDayGrid is not null;

	// Whether the drawn grid exists and shows the current layout; the update passes skip it otherwise.
	bool IsDrawnDayGridShown => isDayGridDrawn && drawnDayGrid is not null;

	/// <summary>
	/// Puts the drawn grid in place of the native one, showing the current layout: its days, titles,
	/// day label style and weekend bands. The grid is created the first time, which RenderLayout leaves
	/// until the calendar is about to be shown: a DayViewTemplate set in XAML after the constructor
	/// switches to the native grid before then, and no drawn grid is built for nothing.
	/// </summary>
	void ShowDrawnDayGrid()
	{
		EnsureDrawnDayGrid();

		drawnDayGrid.IsRightToLeft = IsRightToLeft;
		drawnDayGrid.DayLabelStyle = DaysLabelStyle;
		drawnDayGrid.SetDays([.. dayViews.Select(dayView => (DayModel)dayView.BindingContext)]);
		UpdateDayTitles();
		UpdateWeekendBackground();

		if (drawnDayCanvas is not null && !daysControl.Contains(drawnDayCanvas))
		{
			daysControl.Add(drawnDayCanvas);

			// Back after the native grid was shown (a DayViewTemplate was set and cleared): the canvas
			// attaches its touch handling again, which it lost with its place in the view tree.
			if (drawnDayCanvas is Canvas canvas)
			{
				canvas.Gestures = GesturesMode.Disabled;
				canvas.Gestures = GesturesMode.SoftLock;
			}
		}
	}
}
