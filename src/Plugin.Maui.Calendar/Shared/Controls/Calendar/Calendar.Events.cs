using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Event that is triggered when the month changes.
	/// </summary>
	public event EventHandler<MonthChangedEventArgs> MonthChanged;

	public event EventHandler SwipedLeft;
	public event EventHandler SwipedRight;
	public event EventHandler SwipedUp;
	public event EventHandler SwipedDown;
}
