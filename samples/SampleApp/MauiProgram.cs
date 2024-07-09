using MemoryToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;

namespace SampleApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureMopups()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("font-awesome-5-free-solid.otf", "FontAwesomeSolid");
				fonts.AddFont("font-awesome-5-free-regular.otf", "FontAwesomeRegular");
			});

#if DEBUG
		builder.Logging.AddDebug();
		builder.UseLeakDetection(collectionTarget =>
	{
		// This callback will run any time a leak is detected.
		Application.Current?.MainPage?.DisplayAlert("💦Leak Detected💦",
			$"❗🧟❗{collectionTarget.Name} is a zombie!", "OK");
	});
#endif

		return builder.Build();
	}
}

