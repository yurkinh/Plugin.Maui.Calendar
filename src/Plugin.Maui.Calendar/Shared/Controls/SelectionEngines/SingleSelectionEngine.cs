using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

class SingleSelectionEngine : ISelectionEngine
{
	DateTime? selectedDate;

	internal SingleSelectionEngine() { }

	string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture)
	{
		return selectedDate?.ToString(selectedDateTextFormat, culture);
	}

	bool ISelectionEngine.TryGetSelectedEvents(
		EventCollection allEvents,
		out ICollection selectedEvents
	)
	{
		if (selectedDate.HasValue)
		{
			return allEvents.TryGetValue(selectedDate.Value, out selectedEvents);
		}

		selectedEvents = null;
		return false;
	}

	bool ISelectionEngine.IsDateSelected(DateTime dateToCheck) => dateToCheck == selectedDate;


	List<DateTime> ISelectionEngine.PerformDateSelection(
		DateTime dateToSelect,
		List<DateTime> disabledDates
	)
	{
		if (dateToSelect == selectedDate)
		{
			selectedDate = null;
			return [];
		}
		if (disabledDates is not null && disabledDates.Contains(dateToSelect))
		{
			selectedDate = null;
			return [];
		}

		SelectSingleDate(dateToSelect);

		return [dateToSelect];
	}

	void ISelectionEngine.UpdateDateSelection(List<DateTime> datesToSelect)
	{
		if (datesToSelect?.Count > 0)
		{
			selectedDate = datesToSelect[0];
		}
		else
		{
			selectedDate = null;
		}
	}

	void SelectSingleDate(DateTime? dateToSelect)
	{
		selectedDate = dateToSelect;
	}
}
