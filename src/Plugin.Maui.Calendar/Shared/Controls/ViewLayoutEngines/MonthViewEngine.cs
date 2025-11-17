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
		if (forDate.Year == DateTime.MaxValue.Year && forDate.Month == DateTime.MaxValue.Month)
		{
			return forDate;
		}
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

		long currentMonthIndex = (long)forDate.Year * 12 + (forDate.Month - 1);
		long maxMonthIndex = (long)DateTime.MaxValue.Year * 12 + (DateTime.MaxValue.Month - 1);
		long targetMonthIndex = currentMonthIndex + numberOfUnits;
		if (targetMonthIndex > maxMonthIndex)
		{
			targetMonthIndex = maxMonthIndex;
		}

		int targetYear = (int)(targetMonthIndex / 12);
		int targetMonth = (int)(targetMonthIndex % 12) + 1;
		int targetDay = Math.Min(forDate.Day, DateTime.DaysInMonth(targetYear, targetMonth));

		return new DateTime(targetYear, targetMonth, targetDay, forDate.Hour, forDate.Minute, forDate.Second, forDate.Kind);
	}

	public DateTime GetPreviousUnit(DateTime forDate)
	{
		if (forDate.Year == DateTime.MinValue.Year && forDate.Month == DateTime.MinValue.Month)
		{
			return forDate;
		}
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

		long currentMonthIndex = (long)forDate.Year * 12 + (forDate.Month - 1);
		long minMonthIndex = (long)DateTime.MinValue.Year * 12 + (DateTime.MinValue.Month - 1);
		minMonthIndex = 0;
		long targetMonthIndex = currentMonthIndex - numberOfUnits;
		if (targetMonthIndex < minMonthIndex)
		{
			targetMonthIndex = minMonthIndex;
		}

		int targetYear = (int)(targetMonthIndex / 12);
		int targetMonth = (int)(targetMonthIndex % 12) + 1;
		int targetDay = Math.Min(forDate.Day, DateTime.DaysInMonth(targetYear, targetMonth));

		return new DateTime(targetYear, targetMonth, targetDay, forDate.Hour, forDate.Minute, forDate.Second, forDate.Kind);
	}
}
