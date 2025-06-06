namespace Plugin.Maui.Calendar.Tests.TestableCode.Models;

public class MonthChangedEventArgs(DateOnly oldMonth, DateOnly newMonth) : EventArgs
{
    public DateOnly OldMonth { get; } = oldMonth;
    public DateOnly NewMonth { get; } = newMonth;
}