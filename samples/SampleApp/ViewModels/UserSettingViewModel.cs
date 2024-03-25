namespace SampleApp.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using SampleApp.Services;

public partial class UserSettingViewModel : ObservableObject
{
	readonly IThemeService themeService;
	public UserSettingViewModel(IThemeService themeService)
	{
		this.themeService = themeService;
	}

	[ObservableProperty]
	bool isCheckedLight;

	[ObservableProperty]
	bool isCheckedDark;

	[ObservableProperty]
	bool isCheckedSystem;
	partial void OnIsCheckedLightChanged(bool value)
	{
		themeService.SetTheme((AppTheme)1);
	}
	partial void OnIsCheckedDarkChanged(bool value)
	{
		themeService.SetTheme((AppTheme)2);
	}
	partial void OnIsCheckedSystemChanged(bool value)
	{
		themeService.SetTheme((AppTheme)0);
	}

}
