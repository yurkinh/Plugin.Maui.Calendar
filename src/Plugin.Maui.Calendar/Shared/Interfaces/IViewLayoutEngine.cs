﻿using System.ComponentModel;
using System.Windows.Input;
using Plugin.Maui.Calendar.Controls;

namespace Plugin.Maui.Calendar.Interfaces;

interface IViewLayoutEngine
{
	Grid GenerateLayout(
		List<DayView> dayViews,
		object bindingContext,
		string daysTitleLabelStyleeBindingName,
		ICommand dayTappedCommand
	);

	DateTime GetFirstDate(DateTime dateToShow);

	DateTime GetNextUnit(DateTime forDate);

	DateTime GetNextUnit(DateTime forDate, int numberOfUnits);

	DateTime GetPreviousUnit(DateTime forDate);

	DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits);
}
