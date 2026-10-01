namespace SampleApp.Model;

/// <summary>
/// A line in a day of the Month view: an event, or "+2" for the events that do not fit.
/// </summary>
public class MonthChip
{
	MonthChip(GoogleEvent googleEvent, string text)
	{
		Event = googleEvent;
		Text = text;
	}

	public static MonthChip For(GoogleEvent googleEvent) => new(googleEvent, googleEvent.Title);

	public static MonthChip More(int hiddenCount) => new(null, $"+{hiddenCount}");

	/// <summary>
	/// The event of the chip, null on the "+2" line.
	/// </summary>
	public GoogleEvent Event { get; }

	public string Text { get; }

	public bool IsMore => Event is null;

	public bool IsEvent => Event is not null;
}