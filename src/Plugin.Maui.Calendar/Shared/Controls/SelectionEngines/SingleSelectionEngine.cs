using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.SelectionEngines;

internal class SingleSelectionEngine : ISelectionEngine
{
    private DateTime? _selectedDate;

    internal SingleSelectionEngine() { }

    string ISelectionEngine.GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture)
    {
        return _selectedDate?.ToString(selectedDateTextFormat, culture);
    }

    bool ISelectionEngine.TryGetSelectedEvents(
        EventCollection allEvents,
        out ICollection selectedEvents
    )
    {
        if (_selectedDate.HasValue)
            return allEvents.TryGetValue(_selectedDate.Value, out selectedEvents);

        selectedEvents = null;
        return false;
    }

    bool ISelectionEngine.IsDateSelected(DateTime dateToCheck)
    {
        return Equals(dateToCheck, _selectedDate);
    }

    List<DateTime> ISelectionEngine.PerformDateSelection(
        DateTime dateToSelect,
        List<DateTime>? disabledDates = null
    )
    {
        if (dateToSelect == _selectedDate)
        {
            _selectedDate = null;
            return new List<DateTime>();
        }
        if (disabledDates is not null && disabledDates.Contains(dateToSelect))
        {
            _selectedDate = null;
            return new List<DateTime>();
        }

        SelectSingleDate(dateToSelect);

        return new List<DateTime> { dateToSelect };
    }

    void ISelectionEngine.UpdateDateSelection(List<DateTime> datesToSelect)
    {
        if (datesToSelect?.Count > 0)
            _selectedDate = datesToSelect[0];
        else
            _selectedDate = null;
    }

    private void SelectSingleDate(DateTime? dateToSelect)
    {
        _selectedDate = dateToSelect;
    }
}
