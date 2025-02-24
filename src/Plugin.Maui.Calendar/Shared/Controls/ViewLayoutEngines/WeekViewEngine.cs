using System.Windows.Input;
using Plugin.Maui.Calendar.Interfaces;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

sealed class WeekViewEngine(int numberOfWeeks, DayOfWeek firstDayOfWeek) : ViewLayoutBase(firstDayOfWeek), IViewLayoutEngine
{
	readonly int numberOfWeeks = numberOfWeeks;
	readonly int unitSizeinDays = 7 * numberOfWeeks;

	public Grid GenerateLayout(
		List<DayView> dayViews,
		object bindingContext,
		string weekdayTitleStyleBindingName,
		ICommand dayTappedCommand
	)
	{
		return GenerateWeekLayout(
			dayViews,
			bindingContext,
			weekdayTitleStyleBindingName,
			dayTappedCommand,
			numberOfWeeks
		);
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
