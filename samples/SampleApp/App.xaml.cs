using SampleApp.Services;

namespace SampleApp;

public partial class App : Application
{
	readonly IThemeService themeService;

    public static new App Current => (App)Application.Current;
    public App(IThemeService themeService)
    {
        this.themeService = themeService;
        InitializeComponent();
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