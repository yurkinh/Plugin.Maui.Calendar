namespace Plugin.Maui.Calendar.Models;

public class MonthChangedEventArgs(DateOnly oldMonth, DateOnly newMonth) : EventArgs
{
    public DateOnly OldMonth { get; } = oldMonth;
    public DateOnly NewMonth { get; } = newMonth;
}