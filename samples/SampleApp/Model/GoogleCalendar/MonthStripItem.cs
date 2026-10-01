namespace SampleApp.Model;

/// <summary>
/// An item of the month strip that drops down from the title in the Month view: a month to pick,
/// or the label of a year before its January.
/// </summary>
public partial class MonthStripItem : ObservableObject
{
	MonthStripItem(DateTime date, string text, bool isYear)
	{
		Date = date;
		Text = text;
		IsYear = isYear;
	}

	public static MonthStripItem ForMonth(DateTime firstDay, string text) => new(firstDay, text, false);

	public static MonthStripItem ForYear(DateTime january, string text) => new(january, text, true);

	/// <summary>
	/// First day of the month, or January 1 for a year label.
	/// </summary>
	public DateTime Date { get; }

	public string Text { get; }

	public bool IsYear { get; }

	public bool IsMonth => !IsYear;

	/// <summary>
	/// The month shown by the Month view.
	/// </summary>
	[ObservableProperty]
	public partial bool IsSelected { get; set; }
}