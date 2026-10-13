namespace SampleApp;

public partial class AppShell : Shell
{
	/// <summary>
	/// A Shell route to open at startup: CALROUTE=route (or the calroute intent extra on Android), used
	/// to show a sample page with the drawn day grid of the DrawnUI migration.
	/// </summary>
	public static string StartupRoute { get; set; } = Environment.GetEnvironmentVariable("CALROUTE");

	public AppShell()
	{
		InitializeComponent();
		Loaded += OnLoaded;
	}

	async void OnLoaded(object sender, EventArgs e)
	{
		Loaded -= OnLoaded;

		if (!string.IsNullOrEmpty(StartupRoute))
		{
			await GoToAsync(StartupRoute);
		}
	}
}
