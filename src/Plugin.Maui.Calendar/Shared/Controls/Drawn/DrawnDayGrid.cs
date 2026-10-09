using AppoMobi.Gestures;
using DrawnUi.Draw;
using DrawnUi.Models;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// The drawn counterpart of the native day grid built
/// by <c>ViewLayoutBase.GenerateWeekLayout</c>. A row of day-of-week titles over one
/// <see cref="DrawnDayCell"/> per day, each showing the same <see cref="DayModel"/> a native
/// <see cref="DayView"/> would bind to. Only the built-in cell is drawn; a calendar with a
/// <c>DayViewTemplate</c> keeps the native cells.
/// </summary>
sealed class DrawnDayGrid : SkiaGrid
{
	const int daysInWeek = 7;

	// The distance (in points) a finger moves for a swipe: the default Threshold of MAUI's
	// SwipeGestureRecognizer, which the native cells and the calendar use.
	internal const float SwipeThreshold = 100f;

	readonly List<SkiaLabel> titleLabels = [];
	readonly List<DrawnDayCell> cells = [];

	// The weekend background bands (see Calendar.UpdateWeekendBackground), behind the cells; empty
	// while WeekendDayBackgroundColor is not set.
	readonly List<SkiaShape> weekendBands = [];

	// The styles of the day labels and of the titles, resolved as the native labels resolve them.
	readonly DrawnLabelStyle dayLabelStyle;
	readonly DrawnLabelStyle titleStyle;
	readonly DrawnLabelStyle weekendTitleStyle;

	System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;
	string[] titleTexts = [];
	bool[] weekendTitles = [];
	bool stylesRefreshPending;

	/// <summary>The day cells, in grid order.</summary>
	internal IReadOnlyList<DrawnDayCell> Cells => cells;

	/// <summary>The 7 day-of-week titles, from the first day of the week.</summary>
	internal IReadOnlyList<SkiaLabel> TitleLabels => titleLabels;

	/// <summary>Raised with the cell's day when a day is tapped.</summary>
	internal Action<DayModel> DayTapped { get; set; }

	/// <summary>Raised when a swipe over the grid ends, with its direction.</summary>
	internal Action<SwipeDirection> Swiped { get; set; }

	/// <param name="styleOwner">
	/// The calendar: the styles are resolved below it, so they get its resources and the app's.
	/// </param>
	internal DrawnDayGrid(Element styleOwner)
	{
		dayLabelStyle = new DrawnLabelStyle(styleOwner);
		titleStyle = new DrawnLabelStyle(styleOwner);
		weekendTitleStyle = new DrawnLabelStyle(styleOwner);
		dayLabelStyle.Changed += OnStyleChanged;
		titleStyle.Changed += OnStyleChanged;
		weekendTitleStyle.Changed += OnStyleChanged;

		// The same spacing and columns as the native grid.
		ColumnSpacing = 0;
		RowSpacing = 6;
		ColumnDefinitions = [.. Enumerable.Range(0, daysInWeek).Select(_ => new ColumnDefinition(GridLength.Star))];
		VerticalOptions = LayoutOptions.Start;

		// Cached as a whole: a month change redraws every cell anyway. Not ImageComposite, which
		// re-records only the changed cells but loses the synthetic bold of their labels
		// (FontAttributes Bold without a bold font file) in DrawnUi 1.10.7.3.
		UseCache = SkiaCacheType.Image;

		for (var column = 0; column < daysInWeek; column++)
		{
			// Read as text, as the native title labels are.
			var title = new SkiaLabel { HorizontalOptions = LayoutOptions.Fill, AccessibilityRole = Aria.RoleText };
			Grid.SetColumn(title, column);
			titleLabels.Add(title);
			AddSubView(title);
		}

		// One Tab stop whose days the arrow keys walk, by day and by week (the row length is counted
		// on the first row: the 7 titles).
		AccessibilityRole = Aria.RoleGrid;

		ConsumeGestures += OnGridGestures;
	}

	/// <summary>The culture the screen reader names the days in (the calendar's formatting culture).</summary>
	internal System.Globalization.CultureInfo Culture
	{
		get => culture;
		set
		{
			if (Equals(culture, value))
			{
				return;
			}

			culture = value;
			foreach (var cell in cells)
			{
				cell.Refresh();
			}
		}
	}

	/// <summary>
	/// Shows the days of a new layout. Cells are reused while the number of days stays the same
	/// (a month change only gives them other days); a layout with another number of weeks
	/// creates them again.
	/// </summary>
	internal void SetDays(IReadOnlyList<DayModel> days)
	{
		if (cells.Count != days.Count)
		{
			foreach (var cell in cells)
			{
				RemoveSubView(cell);
				cell.Dispose();
			}
			cells.Clear();

			for (var i = 0; i < days.Count; i++)
			{
				var cell = new DrawnDayCell(day => DayTapped?.Invoke(day), dayLabelStyle, () => culture);
				Grid.SetColumn(cell, i % daysInWeek);
				Grid.SetRow(cell, 1 + (i / daysInWeek));
				cells.Add(cell);
				AddSubView(cell);
			}

			// Bands of the previous layout would cover other rows: the calendar sets them again.
			RemoveWeekendBands();
		}

		for (var i = 0; i < days.Count; i++)
		{
			cells[i].Day = days[i];
		}
	}

	/// <summary>
	/// Lets the cells go of their days while the calendar shows the native grid (a DayViewTemplate
	/// is set). The cells are kept for the next time the grid is shown; <see cref="SetDays"/> gives
	/// them days again.
	/// </summary>
	internal void ReleaseDays()
	{
		foreach (var cell in cells)
		{
			cell.Day = null;
		}
	}

	/// <summary>The weekend background bands, by week and then by column.</summary>
	internal IReadOnlyList<SkiaShape> WeekendBands => weekendBands;

	/// <summary>
	/// Shows a <paramref name="color"/> band behind the weekend days of every week, as the native
	/// grid does (see Calendar.CreateWeekendBands): one per weekend column and week, as wide as the
	/// column, reaching half the row spacing above and below its row so the bands of consecutive
	/// weeks touch, and ignoring touches. A transparent <paramref name="color"/> shows no bands.
	/// </summary>
	internal void SetWeekendBands(Color color, double cornerRadius, DayOfWeek firstDayOfWeek)
	{
		RemoveWeekendBands();

		if (color is not { Alpha: > 0 } || cells.Count == 0)
		{
			return;
		}

		var bleed = RowSpacing / 2;
		for (var week = 1; week <= cells.Count / daysInWeek; week++)
		{
			for (var column = 0; column < daysInWeek; column++)
			{
				if (!ViewLayoutEngines.ViewLayoutBase.IsWeekendColumn(firstDayOfWeek, column))
				{
					continue;
				}

				var band = new SkiaShape
				{
					Type = ShapeType.Rectangle,
					BackgroundColor = color,
					CornerRadius = cornerRadius,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					Margin = new Thickness(0, -bleed, 0, -bleed),
					InputTransparent = true,
				};
				Grid.SetColumn(band, column);
				Grid.SetRow(band, week);
				weekendBands.Add(band);
			}
		}

		// A grid draws its children in the order they were added: the bands go behind the cells.
		foreach (var cell in cells)
		{
			RemoveSubView(cell);
		}
		foreach (var band in weekendBands)
		{
			AddSubView(band);
		}
		foreach (var cell in cells)
		{
			AddSubView(cell);
		}
	}

	void RemoveWeekendBands()
	{
		foreach (var band in weekendBands)
		{
			RemoveSubView(band);
			band.Dispose();
		}

		weekendBands.Clear();
	}

	/// <summary>The style of the day numbers (<c>DaysLabelStyle</c>).</summary>
	internal Style DayLabelStyle
	{
		get => dayLabelStyle.Style;
		set => dayLabelStyle.Style = value;
	}

	/// <summary>
	/// Shows the day-of-week titles (see Calendar.UpdateDayTitles): the weekend days with
	/// <paramref name="weekendStyle"/>, the others with <paramref name="style"/>.
	/// </summary>
	internal void SetTitles(IReadOnlyList<string> texts, IReadOnlyList<bool> weekend, Style style, Style weekendStyle)
	{
		titleTexts = [.. texts];
		weekendTitles = [.. weekend];
		titleStyle.Style = style;
		weekendTitleStyle.Style = weekendStyle;
		ApplyTitles();
	}

	void ApplyTitles()
	{
		for (var i = 0; i < titleLabels.Count && i < titleTexts.Length; i++)
		{
			var style = weekendTitles[i] ? weekendTitleStyle : titleStyle;

			// The native titles are centered unless their style says otherwise (see ViewLayoutBase).
			style.ApplyTo(titleLabels[i], TextAlignment.Center);
			DrawnText.SetText(titleLabels[i], titleTexts[i], style.FontFamily);
		}
	}

	// A new style, theme or resource value: the labels show it once, after all its changes (a new
	// style changes many properties at once).
	void OnStyleChanged()
	{
		if (stylesRefreshPending)
		{
			return;
		}

		if (Dispatcher is not { } dispatcher)
		{
			RefreshStyles();
			return;
		}

		stylesRefreshPending = true;
		dispatcher.Dispatch(() =>
		{
			stylesRefreshPending = false;
			RefreshStyles();
		});
	}

	void RefreshStyles()
	{
		ApplyTitles();

		foreach (var cell in cells)
		{
			cell.Refresh();
		}
	}

	void OnGridGestures(object sender, SkiaGesturesInfo e)
	{
		switch (e.Args.Type)
		{
			// A mostly horizontal pan stays here, to end as a swipe to another month. A vertical one is
			// left to a native ScrollView around the calendar (Canvas.Gestures is SoftLock), as the
			// native cells leave it; without one the grid still gets its end. Taps go to the cells.
			case TouchActionResult.Panning:
				var pan = e.Args.Event.Distance.Total;
				e.Consumed = Math.Abs(pan.X) > Math.Abs(pan.Y);
				break;

			case TouchActionResult.Up:
				var distance = e.Args.Event.Distance.Total;
				if (GetSwipe(distance.X / RenderingScale, distance.Y / RenderingScale) is { } direction)
				{
					Swiped?.Invoke(direction);
				}
				break;
		}
	}

	/// <summary>
	/// The direction of a finger that moved by (<paramref name="x"/>, <paramref name="y"/>) points,
	/// or <see langword="null"/> when it moved less than <see cref="SwipeThreshold"/>: the larger
	/// of the two movements decides, as for MAUI's SwipeGestureRecognizer.
	/// </summary>
	internal static SwipeDirection? GetSwipe(float x, float y)
	{
		if (Math.Max(Math.Abs(x), Math.Abs(y)) < SwipeThreshold)
		{
			return null;
		}

		return Math.Abs(x) >= Math.Abs(y)
			? (x > 0 ? SwipeDirection.Right : SwipeDirection.Left)
			: (y > 0 ? SwipeDirection.Down : SwipeDirection.Up);
	}
}
