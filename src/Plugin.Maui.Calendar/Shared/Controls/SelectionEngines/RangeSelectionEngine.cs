using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

class RangedSelectionEngine : ISelectionEngine
{
	// The first and last day of the selected range (the same day while only its first border is
	// selected), or null when nothing is selected.
	(DateTime Start, DateTime End)? range;

	string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture, bool isNativeDigits)
	{
		if (range is not { } selected)
		{
			return string.Empty;
		}

		return $"{Format(selected.Start)} - {Format(selected.End)}";

		string Format(DateTime date) => isNativeDigits
			? date.ToNativeDigitString(selectedDateTextFormat, culture)
			: date.ToString(selectedDateTextFormat, culture);
	}

	bool ISelectionEngine.TryGetSelectedEvents(EventCollection allEvents, out ICollection selectedEvents)
	{
		var listOfEvents = CreateRangeList();
		return allEvents.TryGetValues(listOfEvents, out selectedEvents);
	}

	bool ISelectionEngine.IsDateSelected(DateTime dateToCheck)
	{
		var date = dateToCheck.Date;
		return range is { } selected && date >= selected.Start && date <= selected.End;
	}

	List<DateTime> ISelectionEngine.PerformDateSelection(DateTime dateToSelect, List<DateTime> disabledDates)
	{
		return SelectDateRange(dateToSelect, disabledDates);
	}

	void ISelectionEngine.UpdateDateSelection(IEnumerable<DateTime> datesToSelect)
	{
		var dates = datesToSelect?.ToList() ?? [];

		range = dates.Count > 0 ? (dates.Min().Date, dates.Max().Date) : null;
	}

	/// <summary>
	/// Selects <paramref name="newSelected"/> as the first border of a new range when no range or a
	/// complete range is selected, or as its second border (the other end) when only the first border
	/// is selected. <see langword="null"/> clears the selection.
	/// </summary>
	internal List<DateTime> SelectDateRange(DateTime? newSelected, List<DateTime> disabledDates)
	{
		if (newSelected is not { } date)
		{
			range = null;
		}
		else if (range is { } selected && selected.Start == selected.End)
		{
			// The second border extends the range before or after the first one.
			range = date.Date <= selected.Start ? (date.Date, selected.End) : (selected.Start, date.Date);
		}
		else
		{
			range = (date.Date, date.Date);
		}

		return CreateRangeList(disabledDates);
	}

	List<DateTime> CreateRangeList(List<DateTime> disabledDates = null)
	{
		if (range is not { } selected)
		{
			return [];
		}

		var rangeList = new List<DateTime>((selected.End - selected.Start).Days + 1);

		// Use a HashSet for O(1) per-day lookups instead of O(n) List.Contains.
		var disabledSet = Calendar.CreateDisabledDateSet(disabledDates);

		// Counted in days, so a range ending on the last day of DateTime does not step past it.
		for (var day = 0; day <= (selected.End - selected.Start).Days; day++)
		{
			var date = selected.Start.AddDays(day);
			if (disabledSet?.Contains(date) != true)
			{
				rangeList.Add(date);
			}
		}

		return rangeList;
	}

	internal List<DateTime> GetDateRange(List<DateTime> disabledDates = null) =>
		CreateRangeList(disabledDates);

	internal DateTime? RangeSelectionStartDate => range?.Start;
	internal DateTime? RangeSelectionEndDate => range?.End;
}
