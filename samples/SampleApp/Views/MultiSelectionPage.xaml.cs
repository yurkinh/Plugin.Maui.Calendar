namespace SampleApp.Views;

public partial class MultiSelectionPage : ContentPage
{
	public MultiSelectionPage()
	{
		InitializeComponent();
	}

	void UnloadedHandler(object sender, EventArgs e)
	{
		calendar.Dispose();
	}
}
