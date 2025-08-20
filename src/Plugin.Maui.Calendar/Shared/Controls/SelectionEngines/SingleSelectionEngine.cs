using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;


namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

class SingleSelectionEngine : ISelectionEngine
{
	DateTime? selectedDate;

	internal SingleSelectionEngine() { }

	string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture, bool isNativeDigits)
	=> isNativeDigits
		? selectedDate?.ToNativeDigitString(selectedDateTextFormat, culture)
		: selectedDate?.ToString(selectedDateTextFormat, culture);

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

	bool ISelectionEngine.IsDateSelected(DateTime dateToCheck) =>
		selectedDate.HasValue && dateToCheck.Date == selectedDate.Value.Date;

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

	void ISelectionEngine.UpdateDateSelection(IEnumerable<DateTime> datesToSelect)
	{
		if (datesToSelect is not null && datesToSelect.Any())
		{
			selectedDate = datesToSelect.First();
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
