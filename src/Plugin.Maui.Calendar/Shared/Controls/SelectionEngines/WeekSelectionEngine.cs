using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

class WeekSelectionEngine(Func<DayOfWeek> firstDayOfWeekProvider) : ISelectionEngine
{
	// The first and last day of the selected week, or null when no week is selected. Both are
	// within the range of DateTime, so the first and last week of DateTime are shorter.
	(DateTime Start, DateTime End)? selectedWeek;

	string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture, bool isNativeDigits)
	{
		if (selectedWeek is not { } week)
		{
			return string.Empty;
		}

		return $"{Format(week.Start)} - {Format(week.End)}";

		string Format(DateTime date) => isNativeDigits
			? date.ToNativeDigitString(selectedDateTextFormat, culture)
			: date.ToString(selectedDateTextFormat, culture);
	}

	bool ISelectionEngine.TryGetSelectedEvents(EventCollection allEvents, out ICollection selectedEvents)
	{
		var selectedDates = CreateSelectedWeekList();
		return allEvents.TryGetValues(selectedDates, out selectedEvents);
	}

	bool ISelectionEngine.IsDateSelected(DateTime dateToCheck)
	{
		var date = dateToCheck.Date;
		return selectedWeek is { } week && date >= week.Start && date <= week.End;
	}

	List<DateTime> ISelectionEngine.PerformDateSelection(DateTime dateToSelect, List<DateTime> disabledDates)
	{
		var selectedDate = dateToSelect.Date;
		var disabledSet = Calendar.CreateDisabledDateSet(disabledDates);

		if (disabledSet?.Contains(selectedDate) == true)
		{
			selectedWeek = null;
			return [];
		}

		var week = GetWeek(selectedDate);

		// Tapping a day of the selected week deselects it.
		if (selectedWeek?.Start == week.Start)
		{
			selectedWeek = null;
			return [];
		}

		selectedWeek = week;

		return CreateSelectedWeekList(disabledSet);
	}

	void ISelectionEngine.UpdateDateSelection(IEnumerable<DateTime> datesToSelect)
	{
		// Cast to DateTime? so an empty sequence gives null rather than DateTime.MinValue, which is a day too.
		var date = datesToSelect?.Cast<DateTime?>().FirstOrDefault();

		selectedWeek = date is { } dateToSelect ? GetWeek(dateToSelect.Date) : null;
	}

	List<DateTime> CreateSelectedWeekList(HashSet<DateTime> disabledSet = null)
	{
		if (selectedWeek is not { } week)
		{
			return [];
		}

		var selectedDates = new List<DateTime>(7);
		for (var day = 0; day <= (week.End - week.Start).Days; day++)
		{
			var date = week.Start.AddDays(day);
			if (disabledSet?.Contains(date) != true)
			{
				selectedDates.Add(date);
			}
		}

		return selectedDates;
	}

	(DateTime Start, DateTime End) GetWeek(DateTime date)
	{
		var daysFromWeekStart = (7 + (date.DayOfWeek - firstDayOfWeekProvider())) % 7;
		var daysSinceMin = (date - DateTime.MinValue).Days;
		var daysUntilMax = (DateTime.MaxValue.Date - date).Days;

		return (
			date.AddDays(-Math.Min(daysFromWeekStart, daysSinceMin)),
			date.AddDays(Math.Min(6 - daysFromWeekStart, daysUntilMax)));
	}
}
