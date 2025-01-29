using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

internal abstract class ViewLayoutBase(CultureInfo culture, DayOfWeek firstDayOfWeek)
{
    protected const int numberOfDaysInWeek = 7;

    public CultureInfo Culture { get; set; } = culture;

    protected DateTime GetFirstDateOfWeek(DateTime dateInWeek)
    {
        var difference = (7 + (dateInWeek.DayOfWeek - firstDayOfWeek)) % 7;
        return dateInWeek.AddDays(-1 * difference).Date;
    }

    protected static Grid GenerateWeekLayout(
            List<DayView> dayViews,
            object bindingContext,
            string daysTitleColorBindingName,
            string daysTitleLabelStyleBindingName,
            ICommand dayTappedCommand,
            PropertyChangedEventHandler dayModelPropertyChanged,
            int numberOfWeeks
        )
    {
        var rowDefinition = new RowDefinition();

        var grid = new Grid
        {
            ColumnSpacing = 0d,
            RowSpacing = 6d,
            RowDefinitions =
            [
                rowDefinition,
            ],
            ColumnDefinitions =
            {
                new ColumnDefinition(){ Width = GridLength.Star},
                new ColumnDefinition(){ Width = GridLength.Star},
                new ColumnDefinition(){ Width = GridLength.Star},
                new ColumnDefinition(){ Width = GridLength.Star},
                new ColumnDefinition(){ Width = GridLength.Star},
                new ColumnDefinition(){ Width = GridLength.Star},
                new ColumnDefinition(){ Width = GridLength.Star},
            }
        };

        for (int i = 0; i < numberOfDaysInWeek; i++)
        {
            var label = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                BindingContext = bindingContext
            };
            label.SetBinding(Label.TextColorProperty, daysTitleColorBindingName);
            label.SetBinding(Label.StyleProperty, daysTitleLabelStyleBindingName);

            grid.Add(label, i, 0);
        }

        dayViews.Clear();

        for (int i = 1; i <= numberOfWeeks; i++)
        {
            rowDefinition = new RowDefinition();
            grid.RowDefinitions.Add(rowDefinition);

            for (int ii = 0; ii < 7; ii++)
            {
                var dayView = new DayView();
                var dayModel = new DayModel();

                dayView.BindingContext = dayModel;
                dayModel.DayTappedCommand = dayTappedCommand;
                dayModel.PropertyChanged += dayModelPropertyChanged;

                dayViews.Add(dayView);
                grid.Add(dayView, ii, i);
            }
        }

        return grid;
    }
}
