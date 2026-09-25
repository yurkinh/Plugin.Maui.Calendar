using System.ComponentModel;
using System.Windows.Input;
using Plugin.Maui.Calendar.Controls;

namespace Plugin.Maui.Calendar.Interfaces;

interface IViewLayoutEngine
{
	/// <summary>
	/// Populates <paramref name="targetGrid"/> with the day-header row and the rows and columns
	/// for the day cells, and fills <paramref name="dayViews"/> with the <see cref="DayView"/>
	/// cells (created with <paramref name="dayViewTemplate"/>), each already assigned its grid row
	/// and column. The cells are not added to <paramref name="targetGrid"/>; the caller adds them
	/// after populating their day models. The caller is responsible for clearing the grid
	/// before invoking this method.
	/// </summary>
	void GenerateLayout(
		Grid targetGrid,
		List<DayView> dayViews,
		object bindingContext,
		string daysTitleLabelStyleeBindingName,
		ICommand dayTappedCommand,
		DataTemplate dayViewTemplate
	);

	DateTime GetFirstDate(DateTime dateToShow);

	DateTime GetLastDate(DateTime dateToShow);

	DateTime GetNextUnit(DateTime forDate);

	DateTime GetNextUnit(DateTime forDate, int numberOfUnits);

	DateTime GetPreviousUnit(DateTime forDate);

	DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits);
}
