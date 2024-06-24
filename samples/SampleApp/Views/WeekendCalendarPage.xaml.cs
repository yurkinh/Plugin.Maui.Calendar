namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class WeekendCalendarPage : ContentPage
{
	public WeekendCalendarPage ()
	{
		InitializeComponent ();

	}
    void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}