namespace Plugin.Maui.Calendar.Models;

public class MonthChangedEventArgs : EventArgs
{
    public int OldMonth { get; }
    public int NewMonth { get; }

    public MonthChangedEventArgs(int oldMonth, int newMonth)
    {
        OldMonth = oldMonth;
        NewMonth = newMonth;
    }
}