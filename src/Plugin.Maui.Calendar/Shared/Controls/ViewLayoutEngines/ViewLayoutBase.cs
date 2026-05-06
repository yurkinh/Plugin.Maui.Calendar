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
	/// Populates <paramref name="targetGrid"/> with the day-of-week header row and
	/// <paramref name="numberOfWeeks"/> × 7 <see cref="DayView"/> cells.
	/// The caller must clear the grid's Children, RowDefinitions and ColumnDefinitions
	/// before calling this method.
	/// </summary>
	protected static void GenerateWeekLayout(
			Grid targetGrid,
			List<DayView> dayViews,
			object bindingContext,
			string daysTitleLabelStyleeBindingName,
			ICommand dayTappedCommand,
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
				var dayView = new DayView();
				var dayModel = new DayModel();
				dayView.BindingContext = dayModel;
				dayModel.DayTappedCommand = dayTappedCommand;

				dayViews.Add(dayView);
				targetGrid.Add(dayView, col, i);
			}
		}
	}
}
