namespace SampleApp.Views;


public partial class WeekViewPage : ContentPage
{
    public WeekViewPage()
    {
        InitializeComponent();
    }

	void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}
