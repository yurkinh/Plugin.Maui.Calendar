using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

namespace Plugin.Maui.Calendar.Controls;

public class MultiSelectionCalendar : Calendar
{

    public MultiSelectionCalendar()
    {
        monthDaysView.CurrentSelectionEngine = new MultiSelectionEngine();
    }
}