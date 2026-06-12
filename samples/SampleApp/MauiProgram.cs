using CommunityToolkit.Maui;
using MemoryToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using SampleApp.Helpers;
using SampleApp.Services;
using SampleApp.Views;
#if DEBUG
using Microsoft.Maui.DevFlow.Agent;
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
            .InjectServices()
            .InjectViewsAndViewModels()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("opensans-regular.ttf", "OpenSansRegular");
				fonts.AddFont("opensans-semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("font-awesome-6-free-solid.otf", "FontAwesomeSolid");
				fonts.AddFont("font-awesome-6-free-regular.otf", "FontAwesomeRegular");
			});


#if DEBUG
		builder.Logging.AddDebug();
		builder.UseMemoryToolkit(options =>
	{
		options.DefaultTearDownStrategy = TearDownStrategy.DisconnectHandlers;
		options.OnLeaked = collectionTarget =>
		{
			// This callback will run any time a leak is detected.
		};
	});

		builder.AddMauiDevFlowAgent();
#endif

        var app = builder.Build();

        //we must initialize our service helper before using it
        ServiceHelper.Initialize(app.Services);

        return app;
    }
    static MauiAppBuilder InjectViewsAndViewModels(this MauiAppBuilder builder)
    {
		builder.Services.AddTransientWithShellRoute<UserSettingPage, UserSettingViewModel>(nameof(UserSettingPage));
		builder.Services.AddTransientWithShellRoute<SimplePage, SimplePageViewModel>(nameof(SimplePage));
		builder.Services.AddTransientWithShellRoute<XiaomiCalendarPage, XiaomiCalendarViewModel>(nameof(XiaomiCalendarPage));
		builder.Services.AddTransientWithShellRoute<EditEventPage, EditEventPageViewModel>(nameof(EditEventPage));
		builder.Services.AddTransientWithShellRoute<TestingPage, TestingPageViewModel>(nameof(TestingPage));

		return builder;
    }
    static MauiAppBuilder InjectServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        return builder;
    }
}

