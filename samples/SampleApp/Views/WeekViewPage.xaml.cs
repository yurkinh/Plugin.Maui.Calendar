namespace SampleApp.Views;


public partial class WeekViewPage : ContentPage
{
    public WeekViewPage()
    {
        InitializeComponent();
    }
    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            calendar.CalendarLayout = Plugin.Maui.Calendar.Enums.WeekLayout.Month;
        }
        else
        {
            calendar.CalendarLayout = Plugin.Maui.Calendar.Enums.WeekLayout.Week;
        }
    }
}
