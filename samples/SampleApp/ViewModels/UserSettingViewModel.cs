using SampleApp.Services;

namespace SampleApp.ViewModels;
public partial class UserSettingViewModel : ObservableObject
{
    readonly IThemeService themeService;
    public UserSettingViewModel(IThemeService themeService)
    {
        this.themeService = themeService;
        InitializeTheme();
    }

    [ObservableProperty]
    bool isCheckedLight;

    [ObservableProperty]
    bool isCheckedDark;

    [ObservableProperty]
    bool isCheckedSystem;

    partial void OnIsCheckedLightChanged(bool value) =>
     themeService.SetTheme(value ? AppTheme.Light : themeService.UserAppTheme);

    partial void OnIsCheckedDarkChanged(bool value) =>
        themeService.SetTheme(value ? AppTheme.Dark : themeService.UserAppTheme);

    partial void OnIsCheckedSystemChanged(bool value) =>
        themeService.SetTheme(value ? AppTheme.Unspecified : themeService.UserAppTheme);

    void InitializeTheme()
    {
        IsCheckedLight = themeService.UserAppTheme == AppTheme.Light;
        IsCheckedDark = themeService.UserAppTheme == AppTheme.Dark;
        IsCheckedSystem = themeService.UserAppTheme == AppTheme.Unspecified;
    }
}
