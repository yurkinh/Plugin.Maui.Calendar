using System.ComponentModel;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// The day grid drawn with Microsoft.Maui.Graphics (the proof of concept of the migration away from
/// DrawnUI): one <see cref="GraphicsView"/> with no views inside it. As the controls of AlohaKit, the
/// view only holds the state and the input: it follows the days and the label styles, gives their
/// values to its <see cref="GraphicsDayGridDrawable"/> (which paints them), is as high as the rows
/// the drawable measures, and gives a tap to the day under it.
/// </summary>
sealed partial class GraphicsDayGrid : GraphicsView, IDrawnDayGrid, IDisposable
{
	// The styles of the day labels and of the titles, resolved as the native labels resolve them.
	readonly DrawnLabelStyle dayLabelStyle;
	readonly DrawnLabelStyle titleStyle;
	readonly DrawnLabelStyle weekendTitleStyle;
	readonly GraphicsDayGridDrawable drawable;

	bool refreshPending;

	public Action<DayModel> DayTapped { get; set; }

	public Action<SwipeDirection> Swiped { get; set; }

	/// <summary>The culture the screen reader names the days in (the calendar's formatting culture).</summary>
	public System.Globalization.CultureInfo Culture
	{
		get => culture;
		set
		{
			if (!Equals(culture, value))
			{
				culture = value;
				Refresh();
			}
		}
	}

	System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;

	/// <param name="styleOwner">
	/// The calendar: the styles are resolved below it, so they get its resources and the app's.
	/// </param>
	internal GraphicsDayGrid(Element styleOwner)
	{
		dayLabelStyle = new DrawnLabelStyle(styleOwner);
		titleStyle = new DrawnLabelStyle(styleOwner);
		weekendTitleStyle = new DrawnLabelStyle(styleOwner);
		dayLabelStyle.Changed += Refresh;
		titleStyle.Changed += Refresh;
		weekendTitleStyle.Changed += Refresh;

		HorizontalOptions = LayoutOptions.Fill;
		VerticalOptions = LayoutOptions.Start;
		Drawable = drawable = new GraphicsDayGridDrawable(dayLabelStyle, titleStyle, weekendTitleStyle);

		// Gesture recognizers rather than the view's own StartInteraction and EndInteraction: the
		// platform then tells a tap from a swipe and from the scrolling of a ScrollView around the
		// calendar, as it does for the native cells.
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, e) =>
		{
			if (e.GetPosition(this) is { } position)
			{
				TapAt(position);
			}
		};
		GestureRecognizers.Add(tap);

#if ANDROID
		// As the native cells (see DayView.AddSwipeForwarding): on Android the view that handles taps
		// takes the whole touch, so the grid recognizes the swipes over it for the calendar.
		foreach (var direction in new[] { SwipeDirection.Left, SwipeDirection.Right, SwipeDirection.Up, SwipeDirection.Down })
		{
			var swipe = new SwipeGestureRecognizer { Direction = direction };
			swipe.Swiped += (_, e) => Swiped?.Invoke(e.Direction);
			GestureRecognizers.Add(swipe);
		}
#endif
	}

	/// <summary>What paints the grid, and knows where its rows and cells are.</summary>
	internal GraphicsDayGridDrawable GridDrawable => drawable;

	public bool IsRightToLeft
	{
		get => drawable.IsRightToLeft;
		set
		{
			if (drawable.IsRightToLeft != value)
			{
				drawable.IsRightToLeft = value;
				Refresh();
			}
		}
	}

	public Style DayLabelStyle
	{
		get => dayLabelStyle.Style;
		set => dayLabelStyle.Style = value;
	}

	public void SetDays(IReadOnlyList<DayModel> days)
	{
		ReleaseDays();

		drawable.Days = days;
		foreach (var day in drawable.Days)
		{
			day.PropertyChanged += OnDayChanged;
		}

		RefreshNow();
	}

	public void ReleaseDays()
	{
		foreach (var day in drawable.Days)
		{
			day.PropertyChanged -= OnDayChanged;
		}

		drawable.Days = [];
	}

	public void SetTitles(IReadOnlyList<string> texts, IReadOnlyList<bool> weekend, Style style, Style weekendStyle)
	{
		drawable.SetTitles(texts, weekend);
		titleStyle.Style = style;
		weekendTitleStyle.Style = weekendStyle;
		RefreshNow();
	}

	public void SetWeekendBands(Color color, double cornerRadius, DayOfWeek firstDayOfWeek)
	{
		drawable.SetWeekendBands(color, (float)cornerRadius, firstDayOfWeek);
		Refresh();
	}

	void OnDayChanged(object sender, PropertyChangedEventArgs e) => Refresh();

	// An update pass of the calendar changes many properties of every day, and a new style many
	// properties of a label: the grid is measured and painted once, after all of them.
	void Refresh()
	{
		if (refreshPending)
		{
			return;
		}

		if (Dispatcher is not { } dispatcher)
		{
			RefreshNow();
			return;
		}

		refreshPending = true;
		dispatcher.Dispatch(() =>
		{
			refreshPending = false;
			RefreshNow();
		});
	}

	// A GraphicsView has no size of its own: it is as high as the rows of the grid.
	void RefreshNow()
	{
		var height = drawable.Measure();
		if (HeightRequest != height)
		{
			HeightRequest = height;
		}

		Invalidate();
		UpdateAccessibility();
	}

	/// <summary>Handles a tap at <paramref name="point"/>, as a touch does.</summary>
	internal void TapAt(Point point)
	{
		if (drawable.DayAt(point, Width) is { } day)
		{
			DayTapped?.Invoke(day);
		}
	}

	public void Dispose()
	{
		ReleaseDays();
		dayLabelStyle.Dispose();
		titleStyle.Dispose();
		weekendTitleStyle.Dispose();
	}
}
