namespace Plugin.Maui.Calendar.UITests.HostApp;

public class App : Application
{
	// The same window size on every run on the desktop, so the calendar is always laid out the same.
	const double desktopWindowWidth = 480;
	const double desktopWindowHeight = 960;

	public App()
	{
		// The calendar's default colors are meant for a light background.
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState) => new(new GalleryPage())
	{
		Title = "Calendar UI Tests",
		Width = desktopWindowWidth,
		Height = desktopWindowHeight,
	};

	/// <summary>Shows <paramref name="page"/> in the app's window, without a navigation animation.</summary>
	public static void Show(Page page) => Current!.Windows[0].Page = page;
}
