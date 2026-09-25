namespace SampleApp.Views;

public partial class MultiSelectionPage : ContentPage
{
	public MultiSelectionPage(SimplePageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
