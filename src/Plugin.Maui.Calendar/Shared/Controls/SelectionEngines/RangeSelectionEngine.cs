using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

class RangedSelectionEngine : ISelectionEngine
{
    DateTime? rangeSelectionEndDate;
    DateTime? rangeSelectionStartDate;

    string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture)
    {
        if (rangeSelectionStartDate.HasValue)
        {
            var startDateText = rangeSelectionStartDate.Value.ToString(
                selectedDateTextFormat,
                culture
            );
            var endDateText = rangeSelectionEndDate.Value.ToString(
                selectedDateTextFormat,
                culture
            );
            return $"{startDateText} - {endDateText}";
        }
        return string.Empty;
    }

    bool ISelectionEngine.TryGetSelectedEvents(
        EventCollection allEvents,
        out ICollection selectedEvents
    )
    {
        var listOfEvents = CreateRangeList();
        return allEvents.TryGetValues(listOfEvents, out selectedEvents);
    }

    bool ISelectionEngine.IsDateSelected(DateTime dateToCheck)
    {
        if (!rangeSelectionStartDate.HasValue)
		{
			return false;
		}

		return DateTime.Compare(dateToCheck, rangeSelectionEndDate.Value.Date) <= 0
            && DateTime.Compare(dateToCheck, rangeSelectionStartDate.Value.Date) >= 0;
    }

    List<DateTime> ISelectionEngine.PerformDateSelection(
        DateTime dateToSelect,
        List<DateTime> disabledDates
    )
    {
        return SelectDateRange(dateToSelect, disabledDates ?? new List<DateTime>());
    }

    void ISelectionEngine.UpdateDateSelection(List<DateTime> datesToSelect)
    {
        if (datesToSelect?.Count > 0)
        {
            rangeSelectionStartDate = datesToSelect[0];
            rangeSelectionEndDate = datesToSelect[0];

            foreach (var date in datesToSelect)
            {
                if (DateTime.Compare(date, rangeSelectionStartDate.Value) < 0)
				{
					rangeSelectionStartDate = date;
				}

				if (DateTime.Compare(rangeSelectionEndDate.Value, date) < 0)
				{
					rangeSelectionEndDate = date;
				}
			}
        }
        else
        {
            rangeSelectionStartDate = null;
            rangeSelectionEndDate = null;
        }
    }

    internal List<DateTime> SelectDateRange(DateTime? newSelected, List<DateTime> disabledDates)
    {
        if (
            rangeSelectionStartDate is null
            || !Equals(rangeSelectionStartDate, rangeSelectionEndDate)
            || newSelected is null
        )
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
            for (
                var currentDate = rangeSelectionStartDate;
                DateTime.Compare(currentDate.Value, rangeSelectionEndDate.Value) <= 0;
                currentDate = currentDate.Value.AddDays(1)
            )
			{
				if (disabledDates is null || !disabledDates.Contains(currentDate.Value))
                {
                    rangeList.Add(currentDate.Value);
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
        if (DateTime.Compare(newSelected.Value.Date, rangeSelectionStartDate.Value.Date) <= 0)
		{
			rangeSelectionStartDate = newSelected.Value.Date;
		}
		else
		{
			rangeSelectionEndDate = newSelected.Value.Date;
		}
	}
}
