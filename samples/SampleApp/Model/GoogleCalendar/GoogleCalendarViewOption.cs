namespace SampleApp.Model;

/// <summary>
/// The views of the Google Calendar look-alike.
/// </summary>
public enum GoogleCalendarView
{
	Schedule = 0,
	Day = 1,
	Week = 2,
	Month = 3,
}

/// <summary>
/// A view in the drawer of the Google Calendar look-alike; the one on screen is highlighted.
/// </summary>
public partial class GoogleCalendarViewOption(GoogleCalendarView view, string title, string glyph) : ObservableObject
{
	public GoogleCalendarView View { get; } = view;

	public string Title { get; } = title;

	/// <summary>
	/// Font Awesome (solid) glyph of the item icon.
	/// </summary>
	public string Glyph { get; } = glyph;

	[ObservableProperty]
	public partial bool IsSelected { get; set; }
}