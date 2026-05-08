using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;

namespace Plugin.Maui.Calendar.Shared.Controls.SelectionEngines;

internal class MultiSelectionEngine : ISelectionEngine
{
	readonly HashSet<DateTime> selectedDates;

	public MultiSelectionEngine()
	{
		selectedDates = [];
	}

	public string GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture, bool isNativeDigits)
	{
		var formatted = selectedDates
			.Where(item => item > DateTime.MinValue)
			.Select(item => isNativeDigits
							? item.ToNativeDigitString(selectedDateTextFormat, culture)
							: item.ToString(selectedDateTextFormat, culture))
			.ToList();

		return formatted.Count == 0 ? string.Empty : string.Join(", ", formatted);
	}

	public bool TryGetSelectedEvents(EventCollection allEvents, out ICollection selectedEvents)
	{
		return allEvents.TryGetValues(selectedDates, out selectedEvents);
	}

	public bool IsDateSelected(DateTime dateToCheck)
	{
		return selectedDates.Contains(dateToCheck);
	}

	public List<DateTime> PerformDateSelection(
		DateTime dateToSelect,
		List<DateTime> disabledDates = null
	)
	{
		if (!selectedDates.Remove(dateToSelect))
		{
			if (disabledDates is null || !disabledDates.Contains(dateToSelect))
			{
				selectedDates.Add(dateToSelect);
			}
		}

		return [.. selectedDates];
	}

	public void UpdateDateSelection(IEnumerable<DateTime> datesToSelect)
	{
		selectedDates.Clear();

		foreach (var date in datesToSelect ?? [])
		{
			selectedDates.Add(date);
		}
	}
}
