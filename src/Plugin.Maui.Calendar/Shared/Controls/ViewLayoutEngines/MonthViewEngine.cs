﻿using System.ComponentModel;
using System.Windows.Input;
using Plugin.Maui.Calendar.Interfaces;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

sealed class MonthViewEngine(DayOfWeek firstDayOfWeek) : ViewLayoutBase(firstDayOfWeek), IViewLayoutEngine
{
	const int monthNumberOfWeeks = 6;

	public Grid GenerateLayout(
		List<DayView> dayViews,
		object bindingContext,
		string daysTitleLabelStyleeBindingName,
		ICommand dayTappedCommand
	)
	{

		return GenerateWeekLayout(
			dayViews,
			bindingContext,
			daysTitleLabelStyleeBindingName,
			dayTappedCommand,
			monthNumberOfWeeks
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
