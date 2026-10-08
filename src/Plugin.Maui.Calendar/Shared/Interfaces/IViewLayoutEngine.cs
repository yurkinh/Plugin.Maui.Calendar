using System.Windows.Input;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Interfaces;

interface IViewLayoutEngine
{
	/// <summary>
	/// Rebuilds <paramref name="targetGrid"/> with the day-header row and all day cells,
	/// filling <paramref name="dayModels"/> with one model per cell.
	/// </summary>
	void GenerateLayout(
		DaysGrid targetGrid,
		List<DayModel> dayModels,
		ICommand dayTappedCommand
	);

	DateTime GetFirstDate(DateTime dateToShow);

	DateTime GetNextUnit(DateTime forDate);

	DateTime GetNextUnit(DateTime forDate, int numberOfUnits);

	DateTime GetPreviousUnit(DateTime forDate);

	DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits);
}
