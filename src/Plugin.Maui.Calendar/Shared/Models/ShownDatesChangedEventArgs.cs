namespace Plugin.Maui.Calendar.Models;

public sealed class ShownDatesChangedEventArgs(DateTime visibleStartDate, DateTime visibleEndDate) : EventArgs
{
	public DateTime VisibleStartDate { get; } = visibleStartDate.Date;
	public DateTime VisibleEndDate { get; } = visibleEndDate.Date;
}
