namespace SampleApp.ViewModels;

partial class UserSettingViewModel:ObservableObject
{
	[ObservableProperty]
	public bool isCheckedLight;

	[ObservableProperty]
	public bool isCheckedDark;

	[ObservableProperty]
	public bool isCheckedSystem;

	partial void OnIsCheckedLightChanged(bool value)
	{
		
	}
	partial void OnIsCheckedDarkChanged(bool value)
	{
		
	}
	partial void OnIsCheckedSystemChanged(bool value)
	{
		
	}
}
