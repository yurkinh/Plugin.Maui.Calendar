using Plugin.Maui.Calendar.Controls.SelectionEngines;

namespace Plugin.Maui.Calendar.Controls;

public class WeekSelectionCalendar : Calendar
{
	public WeekSelectionCalendar()
	{
		CurrentSelectionEngine = new WeekSelectionEngine(() => FirstDayOfWeek);
	}
}
