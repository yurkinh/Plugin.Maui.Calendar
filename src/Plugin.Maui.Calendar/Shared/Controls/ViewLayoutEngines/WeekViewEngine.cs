using System.ComponentModel;
using System.Windows.Input;
using Plugin.Maui.Calendar.Interfaces;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

class WeekViewEngine(int numberOfWeeks, DayOfWeek firstDayOfWeek) : ViewLayoutBase(firstDayOfWeek), IViewLayoutEngine
{
    readonly int numberOfWeeks = numberOfWeeks;
    readonly int unitSizeinDays = 7 * numberOfWeeks;

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
            numberOfWeeks
        );

        return grid;
    }

    public DateTime GetFirstDate(DateTime dateToShow)
    {
        return GetFirstDateOfWeek(dateToShow);
    }

    public DateTime GetNextUnit(DateTime forDate)
    {
        return forDate.AddDays(unitSizeinDays);
    }

    public DateTime GetNextUnit(DateTime forDate, int numberOfUnits)
    {
        return forDate.AddDays(unitSizeinDays * numberOfUnits);
    }

    public DateTime GetPreviousUnit(DateTime forDate)
    {
        return forDate.AddDays(unitSizeinDays * -1);
    }

    public DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits)
    {
        return forDate.AddDays(unitSizeinDays * -1 * numberOfUnits);
    }
}
