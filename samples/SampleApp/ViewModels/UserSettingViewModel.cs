using SampleApp.Services;

namespace SampleApp.ViewModels;
public partial class UserSettingViewModel(IThemeService themeService) : ObservableObject
{
    readonly IThemeService themeService = themeService;

    [ObservableProperty]
    bool isCheckedLight;

    [ObservableProperty]
    bool isCheckedDark;

    [ObservableProperty]
    bool isCheckedSystem = true;
    partial void OnIsCheckedLightChanged(bool value)
    {
        var a = themeService.UserAppTheme;

        if (value)
        {
            themeService.SetTheme(AppTheme.Light);
        }
        var b = themeService.UserAppTheme;
    }
    partial void OnIsCheckedDarkChanged(bool value)
    {
        var a = themeService.UserAppTheme;

        if (value)
        {
            themeService.SetTheme(AppTheme.Dark);
        }
        var b = themeService.UserAppTheme;
    }
    partial void OnIsCheckedSystemChanged(bool value)
    {
        var a = themeService.UserAppTheme;

        if (value)
        {
            themeService.SetTheme(AppTheme.Unspecified);
        }
        var b = themeService.UserAppTheme;
    }
}