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
		string daysTitleLabelStyleeBindingName,
		ICommand dayTappedCommand
	)
	{
		return GenerateWeekLayout(
			dayViews,
			bindingContext,
			daysTitleLabelStyleeBindingName,
			dayTappedCommand,
			numberOfWeeks
		);
	}

	public DateTime GetFirstDate(DateTime dateToShow)
	{
		if (dateToShow == DateTime.MinValue)
		{
			return DateTime.MinValue;
		}
		return GetFirstDateOfWeek(dateToShow);
	}

	public DateTime GetNextUnit(DateTime forDate)
	{
		return GetNextUnit(forDate, 1);
	}

	public DateTime GetNextUnit(DateTime forDate, int numberOfUnits)
	{
		if (numberOfUnits == 0)
		{
			return forDate;
		}
		if (numberOfUnits < 0)
		{
			return GetPreviousUnit(forDate, -numberOfUnits);
		}

		long totalDays = (long)unitSizeinDays * numberOfUnits;
		var daysLeft = (DateTime.MaxValue.Date - forDate.Date).Days;
		var step = (int)Math.Min(daysLeft, Math.Min(int.MaxValue, totalDays));

		var baseDate = forDate.Date.AddDays(step);
		if (baseDate == DateTime.MaxValue.Date)
		{
			return new DateTime(DateTime.MaxValue.Ticks, forDate.Kind);
		}

		var timeTicks = forDate.Ticks - forDate.Date.Ticks;
		return new DateTime(baseDate.Ticks + timeTicks, forDate.Kind);
	}

	public DateTime GetPreviousUnit(DateTime forDate)
	{
		return GetPreviousUnit(forDate, 1);
	}

	public DateTime GetPreviousUnit(DateTime forDate, int numberOfUnits)
	{
		if (numberOfUnits == 0)
		{
			return forDate;
		}
		if (numberOfUnits < 0)
		{
			return GetNextUnit(forDate, -numberOfUnits);
		}

		long totalDays = (long)unitSizeinDays * numberOfUnits;
		var daysToMin = (forDate.Date - DateTime.MinValue.Date).Days;
		var step = (int)Math.Min(daysToMin, Math.Min(int.MaxValue, totalDays));

		var baseDate = forDate.Date.AddDays(-step);
		if (baseDate == DateTime.MinValue.Date)
		{
			return new DateTime(DateTime.MinValue.Ticks, forDate.Kind);
		}

		var timeTicks = forDate.Ticks - forDate.Date.Ticks;
		return new DateTime(baseDate.Ticks + timeTicks, forDate.Kind);
	}
}
