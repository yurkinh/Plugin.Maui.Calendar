﻿using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Plugin.Maui.Calendar.Interfaces;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

internal class MonthViewEngine(CultureInfo culture) : ViewLayoutBase(culture), IViewLayoutEngine
{
    private const int _monthNumberOfWeeks = 6;

    public Grid GenerateLayout(
        List<DayView> dayViews,
        object bindingContext,
        string daysTitleLabelStyleBindingName,
        ICommand dayTappedCommand,
        PropertyChangedEventHandler dayModelPropertyChanged
    )
    {

        return GenerateWeekLayout(
            dayViews,
            bindingContext,
            daysTitleLabelStyleBindingName,
            dayTappedCommand,
            dayModelPropertyChanged,
            _monthNumberOfWeeks
        );
    }

    public DateTime GetFirstDate(DateTime dateToShow)
    {
        return GetFirstDateOfWeek(new DateTime(dateToShow.Year, dateToShow.Month, 1));
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
