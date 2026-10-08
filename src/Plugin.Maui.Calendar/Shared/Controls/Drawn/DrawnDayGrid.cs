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

		// Cached as a whole and re-recorded only where a cell changed (a tap, a selection).
		UseCache = SkiaCacheType.ImageComposite;

		for (var column = 0; column < daysInWeek; column++)
		{
			var title = new SkiaLabel { HorizontalTextAlignment = DrawTextAlignment.Center, HorizontalOptions = LayoutOptions.Fill };
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
			titleLabels[i].Text = texts[i];
			StyleValues.ApplyLabelStyle(titleLabels[i], styles[i]);
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

/// <summary>Reads the values a <see cref="Style"/> (or one it is based on) sets.</summary>
static class StyleValues
{
	internal static T Get<T>(Style style, BindableProperty property, T fallback)
	{
		for (var current = style; current is not null; current = current.BasedOn)
		{
			foreach (var setter in current.Setters)
			{
				if (setter.Property == property && setter.Value is T value)
				{
					return value;
				}
			}
		}

		return fallback;
	}

	/// <summary>
	/// Gives <paramref name="label"/> the text settings of a <see cref="Label"/> style. Spike: only
	/// plain setter values are read, not theme bindings or dynamic resources (phase 3).
	/// </summary>
	internal static void ApplyLabelStyle(SkiaLabel label, Style style)
	{
		label.TextColor = Get(style, Label.TextColorProperty, Colors.Black);
		label.FontSize = Get(style, Label.FontSizeProperty, 14d);
		label.FontAttributes = Get(style, Label.FontAttributesProperty, FontAttributes.None);
		label.FontFamily = Get<string>(style, Label.FontFamilyProperty, null);
		label.Margin = Get(style, View.MarginProperty, new Thickness(0));
	}
}
