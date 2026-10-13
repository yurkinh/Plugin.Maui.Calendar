using SampleApp.Services;

namespace SampleApp;

public partial class App : Application
{
	readonly IThemeService themeService;

    public static new App Current => (App)Application.Current;
    public App(IThemeService themeService, ICalendarSettingsService calendarSettingsService)
    {
        this.themeService = themeService;

        // DrawnUI migration: CALDRAWN=1 (or the caldrawn intent extra on Android) draws the day
        // grids of every page with DrawnUI.
        if (Environment.GetEnvironmentVariable("CALDRAWN") == "1")
        {
            AppContext.SetSwitch("Plugin.Maui.Calendar.DrawnDayGrid", true);
        }

        InitializeComponent();

        // Before the first page is created, so its calendars start with the saved choices
        calendarSettingsService.Apply();
    }

    // The DrawnUI spike benchmark (see DrawnSpikeBenchmarkPage) replaces the shell when requested.
    protected override Window CreateWindow(IActivationState activationState) =>
        new(Views.DrawnSpikeBenchmarkPage.Requested ? new Views.DrawnSpikeBenchmarkPage()
            : Views.DrawnParityPage.Scenario >= 0 ? new Views.DrawnParityPage()
            : new AppShell());

    protected override void OnStart()
    {
        themeService.SetTheme(AppTheme.Unspecified);

#if ANDROID
        UpdateStatusBar();
        RequestedThemeChanged += (_, _) => UpdateStatusBar();
#endif
    }

    protected override void OnResume()
    {

    }

#if ANDROID
    // Set from code: StatusBarBehavior with an AppThemeResource crashes with CommunityToolkit.Maui 15.0.1
    void UpdateStatusBar()
    {
        var isDark = RequestedTheme == AppTheme.Dark;
        var background = (CommunityToolkit.Maui.AppThemeColor)Resources["PageBackgroundColor"];

        CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(isDark ? background.Dark : background.Light);
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetStyle(isDark
            ? CommunityToolkit.Maui.Core.StatusBarStyle.LightContent
            : CommunityToolkit.Maui.Core.StatusBarStyle.DarkContent);
    }
#endif
}
