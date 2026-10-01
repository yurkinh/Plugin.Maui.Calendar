namespace SampleApp.Model;

/// <summary>
/// One calendar of the Google Calendar look-alike ("Personal", "Work", ...), shown or hidden from the drawer.
/// </summary>
public partial class GoogleCalendarSource(string name, Color color) : ObservableObject
{
	public string Name { get; } = name;

	public Color Color { get; } = color;

	[ObservableProperty]
	public partial bool IsVisible { get; set; } = true;
}