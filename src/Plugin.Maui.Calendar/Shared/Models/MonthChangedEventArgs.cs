namespace Plugin.Maui.Calendar.Models;

public class MonthChangedEventArgs(int oldMonth, int newMonth) : EventArgs
{
    public int OldMonth { get; } = oldMonth;
    public int NewMonth { get; } = newMonth;
}