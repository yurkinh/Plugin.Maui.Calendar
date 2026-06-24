namespace Plugin.Maui.Calendar.Tests.TestableCode.Models;

public sealed class ShownDatesChangedEventArgs(DateTime visibleStartDate, DateTime visibleEndDate) : EventArgs
{
	public DateTime VisibleStartDate { get; } = visibleStartDate.Date;
	public DateTime VisibleEndDate { get; } = visibleEndDate.Date;
}
