using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Plugin.Maui.Calendar.Interfaces;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

internal class MonthViewEngine : ViewLayoutBase, IViewLayoutEngine
{
    private const int _monthNumberOfWeeks = 6;

    public MonthViewEngine(CultureInfo culture) : base(culture)
    {
    }

    public Grid GenerateLayout(
        List<DayView> dayViews,
        object bindingContext,
        string daysTitleHeightBindingName,
        string daysTitleColorBindingName,
        string daysTitleLabelStyleBindingName,
        string dayViewSizeBindingName,
        ICommand dayTappedCommand,
        PropertyChangedEventHandler dayModelPropertyChanged
    )
    {
        var grid = GenerateWeekLayout(
            dayViews,
            bindingContext,
            daysTitleHeightBindingName,
            daysTitleColorBindingName,
            daysTitleLabelStyleBindingName,
            dayViewSizeBindingName,
            dayTappedCommand,
            dayModelPropertyChanged,
            _monthNumberOfWeeks
        );

        return grid;
    }

    public DateTime GetFirstDate(DateTime dateToShow)
    {
        var firstOfMonth = new DateTime(dateToShow.Year, dateToShow.Month, 1);
        return GetFirstDateOfWeek(firstOfMonth);        
    }

    public DateTime GetNextUnit(DateTime forDate)
    {
        return forDate.AddMonths(1);
    }

    public DateTime GetNextUnit(DateTime forDate, int numberOfUnits)
    {
        return forDate.AddMonths(numberOfUnits);
    }

    public DateTime GetPreviousUnit(DateTime forDate)
    {
        return forDate.AddMonths(-1);
    }

    public DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits)
    {
        return forDate.AddMonths(numberOfUnits * -1);
    }
}
