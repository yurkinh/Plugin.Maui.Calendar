namespace SampleApp.Views;

public partial class NeoCalendarPage : ContentPage
{
	public NeoCalendarPage()
	{
		InitializeComponent();
	}

	void UnloadedHandler(object sender, EventArgs e)
	{
		calendar.Dispose();
	}
}