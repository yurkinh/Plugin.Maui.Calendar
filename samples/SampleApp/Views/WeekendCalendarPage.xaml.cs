namespace SampleApp.Views;


public partial class WeekendCalendarPage : ContentPage
{
    public WeekendCalendarPage()
    {
        InitializeComponent();

    }
    void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}