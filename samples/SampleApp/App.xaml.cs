using SampleApp.Services;

namespace SampleApp;

public partial class App : Application
{
	readonly IThemeService themeService;

    public static new App Current => (App)Application.Current;
    public App(IThemeService themeService, ICalendarSettingsService calendarSettingsService)
    {
        this.themeService = themeService;
        InitializeComponent();

        // Before the first page is created, so its calendars start with the saved choices
        calendarSettingsService.Apply();
    }

    protected override Window CreateWindow(IActivationState activationState) => new(new AppShell());

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
