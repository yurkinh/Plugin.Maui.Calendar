using Microsoft.Maui.ApplicationModel;

namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class UserSettingPage : ContentPage
{
	public UserSettingPage(UserSettingViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}