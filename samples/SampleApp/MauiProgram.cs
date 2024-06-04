using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using SampleApp.Helpers;
using SampleApp.Services;
using SampleApp.ViewModels;
using SampleApp.Views;

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
            .InjectServices()
            .InjectViewsAndViewModels()
            .ConfigureFonts(fonts =>
			{
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("font-awesome-6-free-solid.otf", "FontAwesomeSolid");
                fonts.AddFont("font-awesome-6-free-regular.otf", "FontAwesomeRegular");
                fonts.AddFont("DarkerGrotesque-VariableFont_wght.ttf", "DarkerGrotesque");
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        //we must initialize our service helper before using it
        ServiceHelper.Initialize(app.Services);

        return app;
    }
    static MauiAppBuilder InjectViewsAndViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddTransient<UserSettingPage>();
        builder.Services.AddTransient<UserSettingViewModel>();
        return builder;
    }
    static MauiAppBuilder InjectServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        return builder;
    }
}

