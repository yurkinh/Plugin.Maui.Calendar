using System.Windows.Input;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

abstract class ViewLayoutBase(DayOfWeek firstDayOfWeek)
{
	protected const int numberOfDaysInWeek = 7;

	/// <summary>
	/// The first day of the week of <paramref name="dateInWeek"/>, or <see cref="DateTime.MinValue"/>
	/// for the first week of <see cref="DateTime"/>, whose first day may lie before it (January 1 of
	/// year 1 is a Monday). The calendar then leaves the cells before <see cref="DateTime.MinValue"/> empty.
	/// </summary>
	protected DateTime GetFirstDateOfWeek(DateTime dateInWeek)
	{
		var difference = GetDaysFromFirstDayOfWeek(dateInWeek);
		return difference > (dateInWeek.Date - DateTime.MinValue).Days
			? DateTime.MinValue
			: dateInWeek.Date.AddDays(-difference);
	}

	/// <summary>
	/// The last day of the <paramref name="numberOfWeeks"/> weeks starting with the week of
	/// <paramref name="dateInFirstWeek"/>, or <see cref="DateTime.MaxValue"/>'s date when they end after it.
	/// </summary>
	protected DateTime GetLastDateOfWeeks(DateTime dateInFirstWeek, int numberOfWeeks)
	{
		// Counted in days since DateTime.MinValue, so a first week that starts before it still ends on the right day.
		long lastDay = (dateInFirstWeek.Date - DateTime.MinValue).Days - GetDaysFromFirstDayOfWeek(dateInFirstWeek) + (numberOfWeeks * numberOfDaysInWeek) - 1;
		long maxDay = (DateTime.MaxValue.Date - DateTime.MinValue).Days;
		return DateTime.MinValue.AddDays(Math.Min(lastDay, maxDay));
	}

	int GetDaysFromFirstDayOfWeek(DateTime date) => (numberOfDaysInWeek + (date.DayOfWeek - firstDayOfWeek)) % numberOfDaysInWeek;

	/// <summary>
	/// Returns <see langword="true"/> when the grid column at <paramref name="column"/>
	/// (0-based, measured from <paramref name="firstDayOfWeek"/>) falls on a Saturday or
	/// Sunday. Used by <see cref="Calendar.UpdateWeekendBackground"/> to place the weekend
	/// background boxes on the same columns as the Saturday/Sunday day-of-week titles.
	/// </summary>
	internal static bool IsWeekendColumn(DayOfWeek firstDayOfWeek, int column)
	{
		int dayNumber = ((int)firstDayOfWeek + column) % numberOfDaysInWeek;
		return dayNumber == (int)DayOfWeek.Saturday || dayNumber == (int)DayOfWeek.Sunday;
	}

	/// <summary>
	/// Populates <paramref name="targetGrid"/> with the day-of-week header row (styled with the
	/// <see cref="Calendar.DaysTitleLabelStyle"/> of <paramref name="calendar"/>) and the rows and
	/// columns for <paramref name="numberOfWeeks"/> × 7 day cells, and fills
	/// <paramref name="dayViews"/> with those <see cref="DayView"/> cells, each already assigned its
	/// grid row and column. The cells are not added to <paramref name="targetGrid"/>: the caller adds
	/// them once their day models hold real data (see <c>Calendar.RenderLayout</c>).
	/// The caller must clear the grid's Children, RowDefinitions and ColumnDefinitions
	/// before calling this method.
	/// </summary>
	protected static void GenerateWeekLayout(
			Grid targetGrid,
			List<DayView> dayViews,
			Calendar calendar,
			ICommand dayTappedCommand,
			DataTemplate dayViewTemplate,
			int numberOfWeeks
	)
	{
		targetGrid.ColumnSpacing = 0d;
		targetGrid.RowSpacing = 6d;

		// Header row (day-of-week titles)
		targetGrid.RowDefinitions.Add(new RowDefinition());

		for (int col = 0; col < numberOfDaysInWeek; col++)
		{
			targetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
		}

		for (int i = 0; i < numberOfDaysInWeek; i++)
		{
			var label = new Label
			{
				HorizontalTextAlignment = TextAlignment.Center
			};
			label.SetBinding(VisualElement.StyleProperty, static (Calendar calendar) => calendar.DaysTitleLabelStyle, source: calendar);

			targetGrid.Add(label, i, 0);
		}

		CreateWeekDays(dayViews, dayTappedCommand, dayViewTemplate, numberOfWeeks);

		for (int i = 1; i <= numberOfWeeks; i++)
		{
			targetGrid.RowDefinitions.Add(new RowDefinition());
		}

		for (int cell = 0; cell < dayViews.Count; cell++)
		{
			Grid.SetColumn(dayViews[cell], cell % numberOfDaysInWeek);
			Grid.SetRow(dayViews[cell], 1 + (cell / numberOfDaysInWeek));
		}
	}

	/// <summary>
	/// Fills <paramref name="dayViews"/> with <paramref name="numberOfWeeks"/> × 7 <see cref="DayView"/>
	/// cells, each with a new <see cref="DayModel"/> as its binding context, in grid order (rows top to
	/// bottom, a row from <c>FirstDayOfWeek</c>). The drawn day grid shows these models without adding
	/// the cells to the screen; <see cref="GenerateWeekLayout"/> places them in the native grid.
	/// </summary>
	protected static void CreateWeekDays(
			List<DayView> dayViews,
			ICommand dayTappedCommand,
			DataTemplate dayViewTemplate,
			int numberOfWeeks
	)
	{
		dayViews.Clear();

		for (int cell = 0; cell < numberOfWeeks * numberOfDaysInWeek; cell++)
		{
			var dayView = new DayView(dayViewTemplate);
			var dayModel = new DayModel();
			dayView.BindingContext = dayModel;
			dayModel.DayTappedCommand = dayTappedCommand;
			dayViews.Add(dayView);
		}
	}
}
