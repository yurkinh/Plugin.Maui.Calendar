namespace SampleApp.Model;

/// <summary>
/// A weekday over the Month view ("Mon"); today's one is highlighted while today is on screen.
/// </summary>
public class WeekdayTitle(int column, string text, bool isToday)
{
	public int Column { get; } = column;

	public string Text { get; } = text;

	public bool IsToday { get; } = isToday;
}