namespace SampleApp.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class UserSettingViewModel : ObservableObject
{
	[ObservableProperty]
	bool isCheckedLight;

	[ObservableProperty]
	bool isCheckedDark;

	[ObservableProperty]
	bool isCheckedSystem;

	void OnIsCheckedLightChanged(object sender, CheckedChangedEventArgs e)
	{

	}
	void OnIsCheckedDarkChanged(object sender, CheckedChangedEventArgs e)
	{

	}
	void OnIsCheckedSystemChanged(object sender, CheckedChangedEventArgs e)
	{

	}
}
