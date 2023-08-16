using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

public class MultiSelectionEngine : ISelectionEngine
{
    private readonly HashSet<DateTime> _selectedDates;

    public MultiSelectionEngine()
    {
        _selectedDates = new HashSet<DateTime>();
    }

    public string GetSelectedDateText(string selectedDateTextFormat, CultureInfo culture)
    {
        return _selectedDates
            .Select(item => item.ToString(selectedDateTextFormat, culture))
            .Aggregate((a, b) => $"{a}, {b}");
    }

    public bool TryGetSelectedEvents(EventCollection allEvents, out ICollection selectedEvents)
    {
        return allEvents.TryGetValues(_selectedDates, out selectedEvents);
    }

    public bool IsDateSelected(DateTime dateToCheck)
    {
        return _selectedDates.Contains(dateToCheck);
    }

    public List<DateTime> PerformDateSelection(DateTime dateToSelect)
    {
        if (_selectedDates.Contains(dateToSelect))
            _selectedDates.Remove(dateToSelect);
        else
            _selectedDates.Add(dateToSelect);

        return _selectedDates.ToList();
    }

    public void UpdateDateSelection(List<DateTime> datesToSelect)
    {
        datesToSelect.ForEach(date => _selectedDates.Add(date));
    }
}

