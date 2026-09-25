namespace SampleApp.Views;


public partial class WeekendFilledCalendarPage : ContentPage
{
    public WeekendFilledCalendarPage(WeekendFilledCalendarPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
