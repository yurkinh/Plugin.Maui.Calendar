namespace Plugin.Maui.Calendar.Controls;

/// <summary>
/// Former native day cell. Days are now drawn with DrawnUI (see <c>Drawn/DayCell</c>);
/// this type is kept only so the public API stays binary compatible and is never instantiated.
/// </summary>
[Obsolete("Days are now drawn; DayView is no longer used.")]
public sealed partial class DayView : ContentView
{
	internal DayView()
	{
	}
}
