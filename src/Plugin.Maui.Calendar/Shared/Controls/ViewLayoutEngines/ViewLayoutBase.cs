using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.ViewLayoutEngines;

internal abstract class ViewLayoutBase
{
    protected const int _numberOfDaysInWeek = 7;

    protected ViewLayoutBase(CultureInfo culture)
    {
        Culture = culture;
    }

    public CultureInfo Culture { get; set; }

    protected DateTime GetFirstDateOfWeek(DateTime dateInWeek)
    {
        var difference = (7 + (dateInWeek.DayOfWeek - Culture.DateTimeFormat.FirstDayOfWeek)) % 7;
        return dateInWeek.AddDays(-1 * difference).Date;
    }

    protected Grid GenerateWeekLayout(
            List<DayView> dayViews,
            object bindingContext,
            string daysTitleHeightBindingName,
            string daysTitleColorBindingName,
            string daysTitleLabelStyleBindingName,
            string dayViewSizeBindingName,
            ICommand dayTappedCommand,
            PropertyChangedEventHandler dayModelPropertyChanged,
            int numberOfWeeks
        )
    {
        var rowDefinition = new RowDefinition()
        {
            BindingContext = bindingContext,
        };
        rowDefinition.SetBinding(RowDefinition.HeightProperty, daysTitleHeightBindingName);

        var columnDefinition = new ColumnDefinition()
        {
            BindingContext = bindingContext,
        };
        columnDefinition.SetBinding(ColumnDefinition.WidthProperty, dayViewSizeBindingName);

        var grid = new Grid
        {
            ColumnSpacing = 0d,
            RowSpacing = 6d,
            RowDefinitions = new RowDefinitionCollection()
            {
                rowDefinition,
            },
            ColumnDefinitions =
            {
                //new ColumnDefinition(){ Width = GridLength.Star},
                //new ColumnDefinition(){ Width = GridLength.Star},
                //new ColumnDefinition(){ Width = GridLength.Star},
                //new ColumnDefinition(){ Width = GridLength.Star},
                //new ColumnDefinition(){ Width = GridLength.Star},
                //new ColumnDefinition(){ Width = GridLength.Star},
                //new ColumnDefinition(){ Width = GridLength.Star},
                columnDefinition,
                columnDefinition,
                columnDefinition,
                columnDefinition,
                columnDefinition,
                columnDefinition,
                columnDefinition
            }
        };

        for (int i = 0; i < _numberOfDaysInWeek; i++)
        {
            var label = new Label()
            {
                HorizontalTextAlignment = TextAlignment.Center,
            };
            label.BindingContext = bindingContext;
            label.SetBinding(Label.TextColorProperty, daysTitleColorBindingName);
            label.SetBinding(Label.StyleProperty, daysTitleLabelStyleBindingName);  

            grid.Add(label, i, 0);
        }

        dayViews.Clear();

        for (int i = 1; i <= numberOfWeeks; i++)
        {
            rowDefinition = new RowDefinition()
            {
                BindingContext = bindingContext,
            };
            rowDefinition.SetBinding(RowDefinition.HeightProperty, dayViewSizeBindingName);
            grid.RowDefinitions.Add(rowDefinition);

            for (int ii = 0; ii < 7; ii++)
            {
                var dayView = new DayView();
                var dayModel = new DayModel();

                dayView.BindingContext = dayModel;
                dayModel.DayTappedCommand = dayTappedCommand;
                dayModel.PropertyChanged += dayModelPropertyChanged;

                //Grid.SetRow(dayView, i);
                //Grid.SetColumn(dayView, ii);

                dayViews.Add(dayView);
                grid.Add(dayView,ii,i);
            }
        }

        return grid;
    }
}
