using System.ComponentModel;
using System.Windows.Input;
using Plugin.Maui.Calendar.Controls;

namespace Plugin.Maui.Calendar.Interfaces;

interface IViewLayoutEngine
{
	/// <summary>
	/// Populates <paramref name="targetGrid"/> with the day-header row and all
	/// <see cref="DayView"/> cells.  The caller is responsible for clearing the grid
	/// before invoking this method.
	/// </summary>
	void GenerateLayout(
		Grid targetGrid,
		List<DayView> dayViews,
		object bindingContext,
		string daysTitleLabelStyleeBindingName,
		ICommand dayTappedCommand
	);

	DateTime GetFirstDate(DateTime dateToShow);

	DateTime GetLastDate(DateTime dateToShow);

	DateTime GetNextUnit(DateTime forDate);

	DateTime GetNextUnit(DateTime forDate, int numberOfUnits);

	DateTime GetPreviousUnit(DateTime forDate);

	DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits);
}
