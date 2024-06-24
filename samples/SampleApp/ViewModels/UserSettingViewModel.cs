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
    bool isCheckedSystem;
    partial void OnIsCheckedLightChanged(bool value)
    {
        if (value)
        {
            themeService.SetTheme((AppTheme)(Themes.Light));
        }
    }
    partial void OnIsCheckedDarkChanged(bool value)
    {
        if (value)
        {
            themeService.SetTheme((AppTheme)(Themes.Dark));
        }
    }
    partial void OnIsCheckedSystemChanged(bool value)
    {
        if (value)
        {
            themeService.SetTheme((AppTheme)(Themes.System));
        }
    }

}