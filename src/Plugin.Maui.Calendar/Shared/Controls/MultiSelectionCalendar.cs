using Plugin.Maui.Calendar.Shared.Controls.SelectionEngines;

namespace Plugin.Maui.Calendar.Controls;

public class MultiSelectionCalendar : Calendar
{
    public MultiSelectionCalendar()
    {
        CurrentSelectionEngine = new MultiSelectionEngine();
    }
}
