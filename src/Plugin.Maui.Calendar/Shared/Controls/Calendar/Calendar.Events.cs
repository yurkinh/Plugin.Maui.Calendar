using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Event that is triggered when the month changes.
	/// </summary>
	public event EventHandler<MonthChangedEventArgs> MonthChanged;

	/// <summary>
	/// Event that is triggered when the shown visible date range changes.
	/// </summary>
	public event EventHandler<ShownDatesChangedEventArgs> ShownDatesChanged;

	public event EventHandler SwipedLeft;
	public event EventHandler SwipedRight;
	public event EventHandler SwipedUp;
	public event EventHandler SwipedDown;

	// Raised with the target position whenever the calendar scrolls its event list; lets the unit
	// tests observe the scrolling, which has no visible effect without a platform handler.
	internal event EventHandler<double> EventsScrollRequested;
}
