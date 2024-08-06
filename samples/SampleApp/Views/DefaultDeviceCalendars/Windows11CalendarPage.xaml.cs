namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
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