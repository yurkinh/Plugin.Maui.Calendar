using System.Windows.Input;
using AppoMobi.Gestures;
using DrawnUi.Draw;
using DrawnUi.Models;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// Drawn replacement for the former <c>daysControl</c> Grid: a 7-column grid with one row of
/// day-of-week titles followed by <c>numberOfWeeks</c> rows of <see cref="DayCell"/>s.
/// <para>
/// Also recognizes swipes made over the days. The canvas owns touches inside it, so the
/// <see cref="SwipeGestureRecognizer"/>s on the calendar would not reliably see them.
/// </para>
/// </summary>
sealed class DaysGrid : SkiaLayout
{
	public const int DaysInWeek = 7;

	// MAUI swipe recognizer defaults to 100; a calendar page flick is usually shorter
	const float swipeThreshold = 50;
	const float swipeDominance = 1.5f;

	readonly List<DayCell> cells = [];
	bool trackingSwipe;

	public DaysGrid()
	{
		Type = LayoutType.Grid;
		HorizontalOptions = LayoutOptions.Fill;
		VerticalOptions = LayoutOptions.Start;
		ColumnSpacing = 0;
		RowSpacing = 6;
		AccessibilityRole = Aria.RoleGrid;
		// recomposes only the regions of cells that changed; each DayCell keeps its own image cache
		UseCache = SkiaCacheType.ImageComposite;

		var columns = new ColumnDefinition[DaysInWeek];
		for (int i = 0; i < DaysInWeek; i++)
		{
			columns[i] = new ColumnDefinition(GridLength.Star);
		}
		ColumnDefinitions = new ColumnDefinitionCollection(columns);
	}

	/// <summary>Raised on the main thread when a swipe ends over the grid.</summary>
	public event EventHandler<SwipeDirection> Swiped;

	public SkiaLabel[] TitleLabels { get; private set; } = [];

	public IReadOnlyList<DayCell> Cells => cells;

	/// <summary>
	/// Creates the title row and <paramref name="numberOfWeeks"/> × 7 day cells, filling
	/// <paramref name="dayModels"/> with their models.
	/// </summary>
	public void Build(int numberOfWeeks, List<DayModel> dayModels, ICommand dayTappedCommand)
	{
		Clear();
		dayModels.Clear();

		var rows = new RowDefinition[numberOfWeeks + 1];
		for (int i = 0; i < rows.Length; i++)
		{
			rows[i] = new RowDefinition(GridLength.Auto);
		}
		RowDefinitions = new RowDefinitionCollection(rows);

		var titles = new SkiaLabel[DaysInWeek];
		for (int col = 0; col < DaysInWeek; col++)
		{
			var title = new SkiaRichLabel
			{
				MarkdownEnabled = false,
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = DrawTextAlignment.Center,
				AccessibilityRole = Aria.RoleText
			};
			Place(title, 0, col);
			titles[col] = title;
		}
		TitleLabels = titles;

		for (int week = 1; week <= numberOfWeeks; week++)
		{
			for (int col = 0; col < DaysInWeek; col++)
			{
				var model = new DayModel { DayTappedCommand = dayTappedCommand };
				var cell = new DayCell(model);
				dayModels.Add(model);
				cells.Add(cell);
				Place(cell, week, col);
			}
		}
	}

	public void Clear()
	{
		foreach (var cell in cells)
		{
			cell.Detach();
		}
		cells.Clear();
		TitleLabels = [];
		ClearChildren();
	}

	public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
	{
		// children first, so day taps keep working
		var consumed = base.ProcessGestures(args, apply);

		switch (args.Type)
		{
			case TouchActionResult.Down:
				trackingSwipe = true;
				break;

			case TouchActionResult.Panning when trackingSwipe:
				var (dx, dy) = Travel(args);
				if (Math.Abs(dx) > Math.Abs(dy) * swipeDominance)
				{
					// keep horizontal pans from a parent ScrollView; vertical ones still scroll it
					return this;
				}
				break;

			case TouchActionResult.Up when trackingSwipe:
				trackingSwipe = false;
				var direction = DetectSwipe(args);
				if (direction is { } swiped)
				{
					RaiseSwiped(swiped);
					return this;
				}
				break;
		}

		return consumed;
	}

	void Place(SkiaControl control, int row, int column)
	{
		Grid.SetRow(control, row);
		Grid.SetColumn(control, column);
		Children.Add(control);
	}

	(float dx, float dy) Travel(SkiaGesturesParameters args)
	{
		var scale = RenderingScale > 0 ? RenderingScale : 1f;
		var start = args.Event.StartingLocation;
		var now = args.Event.Location;
		return ((now.X - start.X) / scale, (now.Y - start.Y) / scale);
	}

	SwipeDirection? DetectSwipe(SkiaGesturesParameters args)
	{
		var (dx, dy) = Travel(args);
		var absX = Math.Abs(dx);
		var absY = Math.Abs(dy);

		if (absX >= swipeThreshold && absX > absY * swipeDominance)
		{
			return dx < 0 ? SwipeDirection.Left : SwipeDirection.Right;
		}

		if (absY >= swipeThreshold && absY > absX * swipeDominance)
		{
			return dy < 0 ? SwipeDirection.Up : SwipeDirection.Down;
		}

		return null;
	}

	void RaiseSwiped(SwipeDirection direction)
	{
		if (MainThread.IsMainThread)
		{
			Swiped?.Invoke(this, direction);
		}
		else
		{
			MainThread.BeginInvokeOnMainThread(() => Swiped?.Invoke(this, direction));
		}
	}
}
