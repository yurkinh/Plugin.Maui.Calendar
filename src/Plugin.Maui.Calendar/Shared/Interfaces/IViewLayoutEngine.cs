using System.ComponentModel;
using System.Windows.Input;
using Plugin.Maui.Calendar.Controls;

namespace Plugin.Maui.Calendar.Interfaces;

internal interface IViewLayoutEngine
{
    Grid GenerateLayout(
        List<DayView> dayViews,
        object bindingContext,
        string daysTitleHeightBindingName,        
        string daysTitleLabelStyleBindingName,
        string dayViewSizeBindingName,
        ICommand dayTappedCommand,
        PropertyChangedEventHandler dayModelPropertyChanged
    );

    DateTime GetFirstDate(DateTime dateToShow);

    DateTime GetNextUnit(DateTime forDate);

    DateTime GetNextUnit(DateTime forDate, int numberOfUnits);

    DateTime GetPreviousUnit(DateTime forDate);

    DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits);
}
