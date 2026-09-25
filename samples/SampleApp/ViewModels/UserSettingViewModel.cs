using System.Globalization;
using SampleApp.Services;

namespace SampleApp.ViewModels;
public partial class UserSettingViewModel : ObservableObject
{
    const string repositoryUrl = "https://github.com/yurkinh/Plugin.Maui.Calendar";

    readonly IThemeService themeService;
    readonly ICalendarSettingsService calendarSettingsService;

    public UserSettingViewModel(IThemeService themeService, ICalendarSettingsService calendarSettingsService)
    {
        this.themeService = themeService;
        this.calendarSettingsService = calendarSettingsService;
        InitializeTheme();

        selectedCulture = calendarSettingsService.Culture;
        selectedFirstDayOfWeek = calendarSettingsService.FirstDayOfWeek;
    }

    [ObservableProperty]
    bool isCheckedLight;

    [ObservableProperty]
    bool isCheckedDark;

    [ObservableProperty]
    bool isCheckedSystem;

    public IReadOnlyList<CultureInfo> Cultures => calendarSettingsService.AvailableCultures;

    public IReadOnlyList<DayOfWeek> FirstDaysOfWeek => calendarSettingsService.AvailableFirstDaysOfWeek;

    [ObservableProperty]
    CultureInfo selectedCulture;

    [ObservableProperty]
    DayOfWeek selectedFirstDayOfWeek;

    partial void OnIsCheckedLightChanged(bool value) =>
     themeService.SetTheme(value ? AppTheme.Light : themeService.UserAppTheme);

    partial void OnIsCheckedDarkChanged(bool value) =>
        themeService.SetTheme(value ? AppTheme.Dark : themeService.UserAppTheme);

    partial void OnIsCheckedSystemChanged(bool value) =>
        themeService.SetTheme(value ? AppTheme.Unspecified : themeService.UserAppTheme);

    // The Picker clears its selection (null) while its items are reset
    partial void OnSelectedCultureChanged(CultureInfo value)
    {
        if (value is not null)
        {
            calendarSettingsService.Culture = value;
        }
    }

    partial void OnSelectedFirstDayOfWeekChanged(DayOfWeek value) =>
        calendarSettingsService.FirstDayOfWeek = value;

    [RelayCommand]
    static Task OpenRepository() => Launcher.Default.OpenAsync(repositoryUrl);

    void InitializeTheme()
    {
        IsCheckedLight = themeService.UserAppTheme == AppTheme.Light;
        IsCheckedDark = themeService.UserAppTheme == AppTheme.Dark;
        IsCheckedSystem = themeService.UserAppTheme == AppTheme.Unspecified;
    }
}
