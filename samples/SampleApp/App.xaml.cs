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
    }

    protected override void OnResume()
    {

    }
}
