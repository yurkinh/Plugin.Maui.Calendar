namespace SampleApp.Views;


public partial class WeekendCalendarPage : ContentPage
{
    public WeekendCalendarPage(WeekendCalendarPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}