using CommunityToolkit.Maui;
using MemoryToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using SampleApp.Helpers;
using SampleApp.Services;
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
        builder.UseLeakDetection(collectionTarget =>
            {
                // This callback will run any time a leak is detected.
                Shell.Current.DisplayAlert("💦Leak Detected💦",
                    $"❗🧟❗{collectionTarget.Name} is a zombie!", "OK");
            });
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

		return builder;
    }
    static MauiAppBuilder InjectServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        return builder;
    }
}

