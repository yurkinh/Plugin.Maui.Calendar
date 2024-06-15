using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

internal class RangedSelectionEngine : ISelectionEngine
{
    private DateTime? _rangeSelectionEndDate;
    private DateTime? _rangeSelectionStartDate;

    string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture)
    {
        if (_rangeSelectionStartDate.HasValue)
        {
            var startDateText = _rangeSelectionStartDate.Value.ToString(
                selectedDateTextFormat,
                culture
            );
            var endDateText = _rangeSelectionEndDate.Value.ToString(
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
        if (!_rangeSelectionStartDate.HasValue)
            return false;

        return DateTime.Compare(dateToCheck, _rangeSelectionEndDate.Value.Date) <= 0
            && DateTime.Compare(dateToCheck, _rangeSelectionStartDate.Value.Date) >= 0;
    }

    List<DateTime> ISelectionEngine.PerformDateSelection(
        DateTime dateToSelect,
        List<DateTime>? disabledDates = null
    )
    {
        return SelectDateRange(dateToSelect, disabledDates);
    }

    void ISelectionEngine.UpdateDateSelection(List<DateTime> datesToSelect)
    {
        if (datesToSelect?.Count > 0)
        {
            _rangeSelectionStartDate = datesToSelect[0];
            _rangeSelectionEndDate = datesToSelect[0];

            foreach (var date in datesToSelect)
            {
                if (DateTime.Compare(date, _rangeSelectionStartDate.Value) < 0)
                    _rangeSelectionStartDate = date;

                if (DateTime.Compare(_rangeSelectionEndDate.Value, date) < 0)
                    _rangeSelectionEndDate = date;
            }
        }
        else
        {
            _rangeSelectionStartDate = null;
            _rangeSelectionEndDate = null;
        }
    }

    internal List<DateTime> SelectDateRange(DateTime? newSelected, List<DateTime>? disabledDates)
    {
        if (
            _rangeSelectionStartDate is null
            || !Equals(_rangeSelectionStartDate, _rangeSelectionEndDate)
            || newSelected is null
        )
            SelectFirstIntervalBorder(newSelected);
        else
            SelectSecondIntervalBorder(newSelected);

        return CreateRangeList(disabledDates);
    }

    private List<DateTime> CreateRangeList(List<DateTime>? disabledDates = null)
    {
        var rangeList = new List<DateTime>();
        if (_rangeSelectionStartDate.HasValue && _rangeSelectionEndDate.HasValue)
        {
            for (
                var currentDate = _rangeSelectionStartDate;
                DateTime.Compare(currentDate.Value, _rangeSelectionEndDate.Value) <= 0;
                currentDate = currentDate.Value.AddDays(1)
            )
                if (disabledDates is null || !disabledDates.Contains(currentDate.Value))
                {
                    rangeList.Add(currentDate.Value);
                }
        }

        return rangeList;
    }

    internal List<DateTime> GetDateRange(List<DateTime>? disabledDates = null) =>
        CreateRangeList(disabledDates);

    private void SelectFirstIntervalBorder(DateTime? newSelected)
    {
        _rangeSelectionStartDate = newSelected?.Date;
        _rangeSelectionEndDate = newSelected?.Date;
    }

    internal DateTime? RangeSelectionStartDate => _rangeSelectionStartDate;
    internal DateTime? RangeSelectionEndDate => _rangeSelectionEndDate;

    private void SelectSecondIntervalBorder(DateTime? newSelected)
    {
        if (DateTime.Compare(newSelected.Value.Date, _rangeSelectionStartDate.Value.Date) <= 0)
            _rangeSelectionStartDate = newSelected.Value.Date;
        else
            _rangeSelectionEndDate = newSelected.Value.Date;
    }
}
