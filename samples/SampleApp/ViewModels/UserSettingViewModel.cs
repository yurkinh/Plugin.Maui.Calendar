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
		SelectedCulture = calendarSettingsService.Culture;
		SelectedFirstDayOfWeek = calendarSettingsService.FirstDayOfWeek;
    }

	[ObservableProperty]
	public partial bool IsCheckedLight { get; set; }

	[ObservableProperty]
	public partial bool IsCheckedDark { get; set; }
	[ObservableProperty]
	public partial bool IsCheckedSystem { get; set; }

	public IReadOnlyList<CultureInfo> Cultures => calendarSettingsService.AvailableCultures;

    public IReadOnlyList<DayOfWeek> FirstDaysOfWeek => calendarSettingsService.AvailableFirstDaysOfWeek;

	[ObservableProperty]
	public partial CultureInfo SelectedCulture { get; set; }

	[ObservableProperty]
	public partial DayOfWeek SelectedFirstDayOfWeek { get; set; }

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
