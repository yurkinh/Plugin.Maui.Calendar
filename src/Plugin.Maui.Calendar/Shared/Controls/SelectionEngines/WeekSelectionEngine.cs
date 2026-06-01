using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

class WeekSelectionEngine(Func<DayOfWeek> firstDayOfWeekProvider) : ISelectionEngine
{
	DateTime? selectedWeekStart;
	DateTime? selectedWeekEnd;

	string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture, bool isNativeDigits)
	{
		if (!selectedWeekStart.HasValue || !selectedWeekEnd.HasValue)
		{
			return string.Empty;
		}

		var startDateText = isNativeDigits
			? selectedWeekStart.Value.ToNativeDigitString(selectedDateTextFormat, culture)
			: selectedWeekStart.Value.ToString(selectedDateTextFormat, culture);
		var endDateText = isNativeDigits
			? selectedWeekEnd.Value.ToNativeDigitString(selectedDateTextFormat, culture)
			: selectedWeekEnd.Value.ToString(selectedDateTextFormat, culture);

		return $"{startDateText} - {endDateText}";
	}

	bool ISelectionEngine.TryGetSelectedEvents(EventCollection allEvents, out ICollection selectedEvents)
	{
		var selectedDates = CreateSelectedWeekList();
		return allEvents.TryGetValues(selectedDates, out selectedEvents);
	}

	bool ISelectionEngine.IsDateSelected(DateTime dateToCheck)
	{
		if (!selectedWeekStart.HasValue || !selectedWeekEnd.HasValue)
		{
			return false;
		}

		var date = dateToCheck.Date;
		return date >= selectedWeekStart.Value.Date && date <= selectedWeekEnd.Value.Date;
	}

	List<DateTime> ISelectionEngine.PerformDateSelection(DateTime dateToSelect, List<DateTime> disabledDates)
	{
		var selectedDate = dateToSelect.Date;
		var disabledSet = CreateDisabledSet(disabledDates);

		if (disabledSet?.Contains(selectedDate) == true)
		{
			ClearSelection();
			return [];
		}

		var weekStart = GetWeekStart(selectedDate);
		var weekEnd = weekStart.AddDays(6);

		if (selectedWeekStart == weekStart && selectedWeekEnd == weekEnd)
		{
			ClearSelection();
			return [];
		}

		selectedWeekStart = weekStart;
		selectedWeekEnd = weekEnd;

		return CreateSelectedWeekList(disabledSet);
	}

	void ISelectionEngine.UpdateDateSelection(IEnumerable<DateTime> datesToSelect)
	{
		var date = datesToSelect?.Select(d => d.Date).FirstOrDefault();

		if (date == default)
		{
			ClearSelection();
			return;
		}

		selectedWeekStart = GetWeekStart(date);
		selectedWeekEnd = selectedWeekStart.Value.AddDays(6);
	}

	List<DateTime> CreateSelectedWeekList(HashSet<DateTime> disabledSet = null)
	{
		if (!selectedWeekStart.HasValue || !selectedWeekEnd.HasValue)
		{
			return [];
		}

		var selectedDates = new List<DateTime>(7);
		for (var date = selectedWeekStart.Value.Date; date <= selectedWeekEnd.Value.Date; date = date.AddDays(1))
		{
			if (disabledSet is null || !disabledSet.Contains(date))
			{
				selectedDates.Add(date);
			}
		}

		return selectedDates;
	}

	DateTime GetWeekStart(DateTime date)
	{
		var firstDayOfWeek = firstDayOfWeekProvider();
		var difference = (7 + (date.DayOfWeek - firstDayOfWeek)) % 7;
		return date.AddDays(-difference).Date;
	}

	static HashSet<DateTime> CreateDisabledSet(List<DateTime> disabledDates)
	{
		if (disabledDates?.Count > 0)
		{
			return [.. disabledDates.Select(date => date.Date)];
		}

		return null;
	}

	void ClearSelection()
	{
		selectedWeekStart = null;
		selectedWeekEnd = null;
	}
}
