using SampleApp.Services;
using SampleApp.Views;
using SampleApp.Model;


namespace SampleApp;

public partial class App : Application
{
	readonly IThemeService themeService;

	public static new App Current => (App)Application.Current;
	public App(IThemeService themeService)
	{
		this.themeService = themeService;
		InitializeComponent();

		MainPage = new NavigationPage(new MainPage());
	}
	protected override void OnStart()
	{
		themeService.SetTheme((AppTheme)(Themes.System));
	}

	protected override void OnResume()
	{
	
	}

}

