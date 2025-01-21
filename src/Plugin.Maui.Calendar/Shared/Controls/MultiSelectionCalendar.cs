using Plugin.Maui.Calendar.Shared.Controls.SelectionEngines;

namespace Plugin.Maui.Calendar.Controls;

public class MultiSelectionCalendar : Calendar
{
    public MultiSelectionCalendar()
    {
        monthDaysView.CurrentSelectionEngine = new MultiSelectionEngine();
    }
}
