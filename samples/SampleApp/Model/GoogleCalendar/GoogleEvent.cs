using System.Globalization;

namespace SampleApp.Model;

/// <summary>
/// An event of the Google Calendar look-alike.
/// </summary>
public class GoogleEvent
{
	public required string Title { get; init; }

	public required DateTime Start { get; init; }

	public DateTime End { get; init; }

	public bool IsAllDay { get; init; }

	public string Location { get; init; }

	public string Description { get; init; }

	public required GoogleCalendarSource Calendar { get; init; }

	/// <summary>
	/// Color picked for this event only; null uses the calendar color, as in Google Calendar.
	/// </summary>
	public Color EventColor { get; init; }

	public Color Color => EventColor ?? Calendar.Color;

	// White reads badly on the light event colors (Banana), so those get dark text
	public Color TextColor => Luminance(Color) > 0.7 ? Color.FromArgb("#1F1F1F") : Colors.White;

	public bool HasTime => !IsAllDay;

	public bool HasLocation => !string.IsNullOrEmpty(Location);

	public bool HasDescription => !string.IsNullOrEmpty(Description);

	/// <summary>
	/// "10:00 – 11:00" in the schedule, empty for an all-day event.
	/// </summary>
	public string TimeText { get; private set; }

	/// <summary>
	/// "Wednesday, October 1 · 10:00 – 11:00" in the event details.
	/// </summary>
	public string DateText { get; private set; }

	public string ReminderText => IsAllDay ? "The day before at 09:00" : "30 minutes before";

	/// <summary>
	/// Formats the time texts with the calendar culture chosen on the Settings tab.
	/// </summary>
	public GoogleEvent Localize(CultureInfo culture)
	{
		TimeText = IsAllDay ? string.Empty : $"{Start.ToString("t", culture)} – {End.ToString("t", culture)}";

		var date = Start.ToString($"dddd, {culture.DateTimeFormat.MonthDayPattern}", culture);
		date = culture.TextInfo.ToUpper(date[0]) + date[1..];
		DateText = IsAllDay ? date : $"{date} · {TimeText}";

		return this;
	}

	static double Luminance(Color color) => 0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue;
}