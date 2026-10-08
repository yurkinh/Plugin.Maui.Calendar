using AppoMobi.Gestures;
using DrawnUi.Draw;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Phase 0 spike of the hybrid DrawnUI migration: the drawn counterpart of the native day grid built
/// by <c>ViewLayoutBase.GenerateWeekLayout</c>. A row of day-of-week titles over one
/// <see cref="DrawnDayCell"/> per day, each showing the same <see cref="DayModel"/> a native
/// <see cref="DayView"/> would bind to. Only the built-in cell is drawn; a calendar with a
/// <c>DayViewTemplate</c> keeps the native cells.
/// </summary>
sealed class DrawnDayGrid : SkiaGrid
{
	const int daysInWeek = 7;

	// Swipes shorter than this (in points) are taps or jitter, not swipes.
	const float swipeThreshold = 40f;

	readonly List<SkiaLabel> titleLabels = [];
	readonly List<DrawnDayCell> cells = [];

	/// <summary>The day cells, in grid order.</summary>
	internal IReadOnlyList<DrawnDayCell> Cells => cells;

	/// <summary>The 7 day-of-week titles, from the first day of the week.</summary>
	internal IReadOnlyList<SkiaLabel> TitleLabels => titleLabels;

	/// <summary>Raised with the cell's day when a day is tapped.</summary>
	internal Action<DayModel> DayTapped { get; set; }

	/// <summary>Raised when a swipe over the grid ends, with its direction.</summary>
	internal Action<SwipeDirection> Swiped { get; set; }

	internal DrawnDayGrid()
	{
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
			var title = new SkiaLabel { HorizontalOptions = LayoutOptions.Fill };
			Grid.SetColumn(title, column);
			titleLabels.Add(title);
			AddSubView(title);
		}

		ConsumeGestures += OnGridGestures;
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
				var cell = new DrawnDayCell(day => DayTapped?.Invoke(day));
				Grid.SetColumn(cell, i % daysInWeek);
				Grid.SetRow(cell, 1 + (i / daysInWeek));
				cells.Add(cell);
				AddSubView(cell);
			}
		}

		for (var i = 0; i < days.Count; i++)
		{
			cells[i].Day = days[i];
		}
	}

	/// <summary>Shows the day-of-week titles with their label styles (see Calendar.UpdateDayTitles).</summary>
	internal void SetTitles(IReadOnlyList<string> texts, IReadOnlyList<Style> styles)
	{
		for (var i = 0; i < titleLabels.Count && i < texts.Count; i++)
		{
			// The native titles are centered unless their style says otherwise (see ViewLayoutBase).
			DrawnText.ApplyLabelStyle(titleLabels[i], styles[i], DrawnText.DefaultLabelFontSize, TextAlignment.Center);
			DrawnText.SetText(titleLabels[i], texts[i], styles[i]);
		}
	}

	void OnGridGestures(object sender, SkiaGesturesInfo e)
	{
		switch (e.Args.Type)
		{
			// Keeps a pan, so the swipe ends here. Taps are left to the cells.
			case TouchActionResult.Panning:
				e.Consumed = true;
				break;

			case TouchActionResult.Up:
				var distance = e.Args.Event.Distance.Total;
				var threshold = swipeThreshold * RenderingScale;
				if (Math.Max(Math.Abs(distance.X), Math.Abs(distance.Y)) >= threshold)
				{
					Swiped?.Invoke(Math.Abs(distance.X) >= Math.Abs(distance.Y)
						? (distance.X > 0 ? SwipeDirection.Right : SwipeDirection.Left)
						: (distance.Y > 0 ? SwipeDirection.Down : SwipeDirection.Up));
				}
				break;
		}
	}
}
