using CommunityToolkit.Mvvm.ComponentModel;
using SampleApp.Services;
using SampleApp.Model;

namespace SampleApp.ViewModels;
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
