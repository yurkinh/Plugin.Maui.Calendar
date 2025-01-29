using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Plugin.Maui.Calendar.Interfaces;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

internal class WeekViewEngine(CultureInfo culture, int numberOfWeeks, DayOfWeek firstDayOfWeek) : ViewLayoutBase(culture, firstDayOfWeek), IViewLayoutEngine
{
    private readonly int _numberOfWeeks = numberOfWeeks;
    private readonly int _unitSizeinDays = 7 * numberOfWeeks;

    public Grid GenerateLayout(
        List<DayView> dayViews,
        object bindingContext,
        string daysTitleColorBindingName,
        string daysTitleLabelStyleBindingName,
        ICommand dayTappedCommand,
        PropertyChangedEventHandler dayModelPropertyChanged
    )
    {
        var grid = GenerateWeekLayout(
            dayViews,
            bindingContext,
            daysTitleColorBindingName,
            daysTitleLabelStyleBindingName,
            dayTappedCommand,
            dayModelPropertyChanged,
            _numberOfWeeks
        );

        return grid;
    }

    public DateTime GetFirstDate(DateTime dateToShow)
    {
        return GetFirstDateOfWeek(dateToShow);
    }

    public DateTime GetNextUnit(DateTime forDate)
    {
        return forDate.AddDays(_unitSizeinDays);
    }

    public DateTime GetNextUnit(DateTime forDate, int numberOfUnits)
    {
        return forDate.AddDays(_unitSizeinDays * numberOfUnits);
    }

    public DateTime GetPreviousUnit(DateTime forDate)
    {
        return forDate.AddDays(_unitSizeinDays * -1);
    }

    public DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits)
    {
        return forDate.AddDays(_unitSizeinDays * -1 * numberOfUnits);
    }
}
