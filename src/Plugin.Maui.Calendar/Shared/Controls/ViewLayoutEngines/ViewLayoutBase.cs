using System.Windows.Input;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

abstract class ViewLayoutBase(DayOfWeek firstDayOfWeek)
{
	protected const int numberOfDaysInWeek = 7;

	protected DateTime GetFirstDateOfWeek(DateTime dateInWeek)
	{
		var difference = (7 + (dateInWeek.DayOfWeek - firstDayOfWeek)) % 7;
		return dateInWeek.AddDays(-1 * difference).Date;
	}

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
	/// Populates <paramref name="targetGrid"/> with the day-of-week header row and the rows and
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
			object bindingContext,
			string daysTitleLabelStyleeBindingName,
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
				HorizontalTextAlignment = TextAlignment.Center,
				BindingContext = bindingContext
			};
			label.SetBinding(VisualElement.StyleProperty, daysTitleLabelStyleeBindingName);

			targetGrid.Add(label, i, 0);
		}

		dayViews.Clear();

		for (int i = 1; i <= numberOfWeeks; i++)
		{
			targetGrid.RowDefinitions.Add(new RowDefinition());

			for (int col = 0; col < numberOfDaysInWeek; col++)
			{
				var dayView = new DayView(dayViewTemplate);
				var dayModel = new DayModel();
				dayView.BindingContext = dayModel;
				dayModel.DayTappedCommand = dayTappedCommand;

				Grid.SetColumn(dayView, col);
				Grid.SetRow(dayView, i);
				dayViews.Add(dayView);
			}
		}
	}
}
