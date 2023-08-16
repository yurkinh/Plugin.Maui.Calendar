using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

namespace Plugin.Maui.Calendar.Controls;

public class MultiSelectionCalendar : Calendar
{
    private readonly MultiSelectionEngine _multiSelectionEngine;

    public MultiSelectionCalendar()
    {
        monthDaysView.CurrentSelectionEngine = _multiSelectionEngine = new MultiSelectionEngine();
    }
}