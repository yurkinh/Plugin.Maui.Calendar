using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using CommunityToolkit.Maui;



#if DEBUG
using DotNet.Meteor.HotReload.Plugin;
#endif

namespace SampleApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureMopups()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("font-awesome-6-free-solid-900.otf", "FontAwesomeSolid");
				fonts.AddFont("font-awesome-6-free-regular-400.otf", "FontAwesomeRegular");
			});

#if DEBUG
		builder.Logging.AddDebug();
		builder.EnableHotReload();
#endif

		return builder.Build();
	}
}
