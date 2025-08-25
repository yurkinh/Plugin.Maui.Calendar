namespace SampleApp.Views;

public partial class TestingPage : ContentPage
{
	public TestingPage(TestingPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}