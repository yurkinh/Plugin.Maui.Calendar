using DrawnUi.Draw;

namespace Plugin.Maui.Calendar.UITests.HostApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		// The drawn day grid of the calendar (see GalleryPage.DrawnButton).
		builder.UseDrawnUi();
		return builder.Build();
	}
}
