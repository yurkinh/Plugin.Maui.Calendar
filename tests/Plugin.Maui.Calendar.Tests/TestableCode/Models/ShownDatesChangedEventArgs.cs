namespace Plugin.Maui.Calendar.Tests.TestableCode.Models;

public class ShownDatesChangedEventArgs(DateTime visibleStartDate, DateTime visibleEndDate) : EventArgs
{
	public DateTime VisibleStartDate { get; } = visibleStartDate.Date;
	public DateTime VisibleEndDate { get; } = visibleEndDate.Date;
}
