namespace SampleApp.Model;

/// <summary>
/// A day on the hours of the Day and Week views: its all-day events above the hours, its timed events on them.
/// </summary>
public class DayColumn(int column, DateTime date, string weekdayText, string dayText, IReadOnlyList<GoogleEvent> allDayEvents, IReadOnlyList<EventBlock> blocks, double hourHeight)
{
	/// <summary>
	/// Grid column of the day in the Week view, 0 for the first day of the week.
	/// </summary>
	public int Column { get; } = column;

	public DateTime Date { get; } = date;

	public string WeekdayText { get; } = weekdayText;

	public string DayText { get; } = dayText;

	public bool IsToday => Date == DateTime.Today;

	public IReadOnlyList<GoogleEvent> AllDayEvents { get; } = allDayEvents;

	public bool HasAllDayEvents => AllDayEvents.Count > 0;

	public IReadOnlyList<EventBlock> Blocks { get; } = blocks;

	/// <summary>
	/// Places the current time line (10 high, centered on the time) on today.
	/// </summary>
	public Thickness NowLineMargin { get; } = new(0, DateTime.Now.TimeOfDay.TotalHours * hourHeight - 5, 0, 0);
}

/// <summary>
/// A timed event placed on the hours of the Day and Week views. Events that overlap share the width of the day.
/// </summary>
public class EventBlock
{
	public EventBlock(GoogleEvent googleEvent, int lane, int laneCount, double hourHeight)
	{
		Event = googleEvent;

		// Cut at midnight: every day is shown on its own
		var dayEnd = googleEvent.Start.Date.AddDays(1);
		var end = googleEvent.End < dayEnd ? googleEvent.End : dayEnd;
		var top = googleEvent.Start.TimeOfDay.TotalHours * hourHeight;
		var height = Math.Max((end - googleEvent.Start).TotalHours * hourHeight - 2, 20);

		// With XProportional the layout places a child at (width - child width) * X,
		// so lane i of n starts at i / (n - 1)
		Bounds = new Rect(laneCount > 1 ? lane / (laneCount - 1.0) : 0, top, 1.0 / laneCount, height);
	}

	public GoogleEvent Event { get; }

	/// <summary>
	/// AbsoluteLayout bounds: proportional X and width, top and height in device-independent units.
	/// </summary>
	public Rect Bounds { get; }
}