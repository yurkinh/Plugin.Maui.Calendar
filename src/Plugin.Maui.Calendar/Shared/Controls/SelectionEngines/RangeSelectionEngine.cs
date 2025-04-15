using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

class RangedSelectionEngine : ISelectionEngine
{
	DateTime? rangeSelectionEndDate;
	DateTime? rangeSelectionStartDate;

	string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture)
	{
		if (rangeSelectionStartDate.HasValue && rangeSelectionEndDate.HasValue)
		{
			var startDateText = rangeSelectionStartDate.Value.ToLocalizedString(selectedDateTextFormat, culture);
			var endDateText = rangeSelectionEndDate.Value.ToLocalizedString(selectedDateTextFormat, culture);
			return $"{startDateText} - {endDateText}";
		}
		return string.Empty;
	}

	bool ISelectionEngine.TryGetSelectedEvents(EventCollection allEvents, out ICollection selectedEvents)
	{
		var listOfEvents = CreateRangeList();
		return allEvents.TryGetValues(listOfEvents, out selectedEvents);
	}

	bool ISelectionEngine.IsDateSelected(DateTime dateToCheck)
	{
		if (!rangeSelectionStartDate.HasValue || !rangeSelectionEndDate.HasValue)
		{
			return false;
		}

		var date = dateToCheck.Date;
		return date >= rangeSelectionStartDate.Value.Date && date <= rangeSelectionEndDate.Value.Date;
	}

	List<DateTime> ISelectionEngine.PerformDateSelection(DateTime dateToSelect, List<DateTime> disabledDates)
	{
		return SelectDateRange(dateToSelect, disabledDates ?? new List<DateTime>());
	}

	void ISelectionEngine.UpdateDateSelection(List<DateTime> datesToSelect)
	{
		if (datesToSelect?.Count > 0)
		{
			// Use LINQ to simplify finding min and max
			rangeSelectionStartDate = datesToSelect.Min().Date;
			rangeSelectionEndDate = datesToSelect.Max().Date;
		}
		else
		{
			rangeSelectionStartDate = null;
			rangeSelectionEndDate = null;
		}
	}

	internal List<DateTime> SelectDateRange(DateTime? newSelected, List<DateTime> disabledDates)
	{
		if (newSelected is null
			|| !rangeSelectionStartDate.HasValue
			|| rangeSelectionStartDate != rangeSelectionEndDate)
		{
			SelectFirstIntervalBorder(newSelected);
		}
		else
		{
			SelectSecondIntervalBorder(newSelected);
		}

		return CreateRangeList(disabledDates);
	}

	List<DateTime> CreateRangeList(List<DateTime> disabledDates = null)
	{
		var rangeList = new List<DateTime>();

		if (rangeSelectionStartDate.HasValue && rangeSelectionEndDate.HasValue)
		{
			for (var date = rangeSelectionStartDate.Value.Date; date <= rangeSelectionEndDate.Value.Date; date = date.AddDays(1))
			{
				if (disabledDates == null || !disabledDates.Contains(date))
				{
					rangeList.Add(date);
				}
			}
		}

		return rangeList;
	}

	internal List<DateTime> GetDateRange(List<DateTime> disabledDates = null) =>
		CreateRangeList(disabledDates);

	void SelectFirstIntervalBorder(DateTime? newSelected)
	{
		rangeSelectionStartDate = newSelected?.Date;
		rangeSelectionEndDate = newSelected?.Date;
	}

	internal DateTime? RangeSelectionStartDate => rangeSelectionStartDate;
	internal DateTime? RangeSelectionEndDate => rangeSelectionEndDate;

	void SelectSecondIntervalBorder(DateTime? newSelected)
	{
		if (newSelected is null)
		{
			return;
		}

		var newDate = newSelected.Value.Date;
		// If new date is before or equal to start, update start; otherwise update end.
		if (!rangeSelectionStartDate.HasValue || newDate <= rangeSelectionStartDate.Value.Date)
		{
			rangeSelectionStartDate = newDate;
		}
		else
		{
			rangeSelectionEndDate = newDate;
		}
	}
}
