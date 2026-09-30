namespace SampleApp.Model;

/// <summary>
/// A row of the schedule list of the Google Calendar look-alike. Rows are ordered by <see cref="Date"/>.
/// </summary>
public abstract class ScheduleRow(DateTime date)
{
	public DateTime Date { get; } = date;
}

/// <summary>
/// The illustrated banner that opens every month.
/// </summary>
public class ScheduleMonthRow : ScheduleRow
{
	// Sky top, sky bottom, sun, far hill, near hill: a simple flat scene per month, like Google's month artwork
	static readonly string[][] scenes =
	[
		["#CFE3F7", "#F3F8FD", "#FFFFFF", "#B3CDE8", "#E4EEF8"],
		["#F8D9E2", "#FDF3F6", "#F48FB1", "#E7B3C3", "#F2CFDA"],
		["#D8EFD4", "#F3FAF1", "#FFE082", "#A5D6A7", "#7CB982"],
		["#D5EEF6", "#F4FBFD", "#FFD54F", "#9CCC65", "#72B043"],
		["#DDF1DE", "#F5FBF5", "#FFCA28", "#81C784", "#4FA457"],
		["#D3EFFC", "#F4FCFF", "#FFB300", "#80CBC4", "#43A89D"],
		["#FFE9C7", "#FFF9EF", "#FF8F00", "#4FC3F7", "#1FA6E6"],
		["#FFF0C2", "#FFFAEB", "#FFA000", "#FFE082", "#F9C846"],
		["#FFE7C9", "#FFF8EF", "#FFB74D", "#C5E1A5", "#9DCB6F"],
		["#FCDCD4", "#FFF5F2", "#FF8A65", "#FFAB91", "#F4713F"],
		["#E9E1DD", "#FAF7F5", "#BCAAA4", "#A1887F", "#80675D"],
		["#D6E9FB", "#F6FAFF", "#FFFFFF", "#B7D8F7", "#E6F1FC"],
	];

	public ScheduleMonthRow(DateTime date, string title) : base(date)
	{
		Title = title;

		var scene = scenes[date.Month - 1];
		SkyBrush = new LinearGradientBrush(
			[new GradientStop(Color.FromArgb(scene[0]), 0), new GradientStop(Color.FromArgb(scene[1]), 1)],
			new Point(0, 0),
			new Point(0, 1));
		SunColor = Color.FromArgb(scene[2]);
		FarHillColor = Color.FromArgb(scene[3]);
		NearHillColor = Color.FromArgb(scene[4]);
	}

	public string Title { get; }

	public Brush SkyBrush { get; }

	public Color SunColor { get; }

	public Color FarHillColor { get; }

	public Color NearHillColor { get; }
}

/// <summary>
/// The "Oct 5 – 11" label over every week.
/// </summary>
public class ScheduleWeekRow(DateTime date, string text) : ScheduleRow(date)
{
	public string Text { get; } = text;
}

/// <summary>
/// A day with events: the date on the left, the event chips on the right.
/// </summary>
public class ScheduleDayRow(DateTime date, string weekdayText, string dayText, IReadOnlyList<object> items) : ScheduleRow(date)
{
	public string WeekdayText { get; } = weekdayText;

	public string DayText { get; } = dayText;

	public bool IsToday => Date == DateTime.Today;

	/// <summary>
	/// The day's <see cref="GoogleEvent"/>s, and a <see cref="ScheduleNowLine"/> on today.
	/// </summary>
	public IReadOnlyList<object> Items { get; } = items;
}

/// <summary>
/// The red line that marks the current time between today's events.
/// </summary>
public class ScheduleNowLine;