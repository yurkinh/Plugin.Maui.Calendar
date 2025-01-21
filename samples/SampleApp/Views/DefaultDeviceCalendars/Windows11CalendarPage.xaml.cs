namespace SampleApp.Views;


public partial class Windows11CalendarPage : ContentPage
{
    public Windows11CalendarPage()
    {
        InitializeComponent();
    }
    void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}