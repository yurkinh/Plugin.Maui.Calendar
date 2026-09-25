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
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
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

#if IOS || MACCATALYST
        // Pickers sit in settings rows and show the picked value as plain text, without a text field border
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoBorder", (handler, _) =>
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None);
#endif

        var app = builder.Build();

        //we must initialize our service helper before using it
        ServiceHelper.Initialize(app.Services);

        return app;
    }
    static MauiAppBuilder InjectViewsAndViewModels(this MauiAppBuilder builder)
    {
		// Tab roots: AppShell.xaml already gives them their routes
		builder.Services.AddTransient<MainPage, MainPageViewModel>();
		builder.Services.AddTransient<UserSettingPage, UserSettingViewModel>();

		builder.Services.AddTransientWithShellRoute<CalendarPage>(nameof(CalendarPage));
		builder.Services.AddTransientWithShellRoute<SimplePage, SimplePageViewModel>(nameof(SimplePage));
		builder.Services.AddTransientWithShellRoute<AdvancedPage, AdvancedPageViewModel>(nameof(AdvancedPage));
		builder.Services.AddTransientWithShellRoute<MultiSelectionPage, SimplePageViewModel>(nameof(MultiSelectionPage));
		builder.Services.AddTransientWithShellRoute<RangeSelectionPage, RangeSelectionPageViewModel>(nameof(RangeSelectionPage));
		builder.Services.AddTransientWithShellRoute<WeekSelectionPage>(nameof(WeekSelectionPage));
		builder.Services.AddTransientWithShellRoute<WeekendCalendarPage, WeekendCalendarPageViewModel>(nameof(WeekendCalendarPage));
		builder.Services.AddTransientWithShellRoute<WeekendFilledCalendarPage, WeekendFilledCalendarPageViewModel>(nameof(WeekendFilledCalendarPage));
		builder.Services.AddTransientWithShellRoute<DayViewTemplatePage, DayViewTemplatePageViewModel>(nameof(DayViewTemplatePage));
		builder.Services.AddTransientWithShellRoute<WeekViewPage, WeekViewPageViewModel>(nameof(WeekViewPage));
		builder.Services.AddTransientWithShellRoute<TwoWeekViewPage, TwoWeekViewPageViewModel>(nameof(TwoWeekViewPage));
		builder.Services.AddTransientWithShellRoute<Windows11CalendarPage, Windows11CalendarViewModel>(nameof(Windows11CalendarPage));
		builder.Services.AddTransientWithShellRoute<XiaomiCalendarPage, XiaomiCalendarViewModel>(nameof(XiaomiCalendarPage));
		builder.Services.AddTransientWithShellRoute<EditEventPage, EditEventPageViewModel>(nameof(EditEventPage));
		builder.Services.AddTransientWithShellRoute<TestingPage, TestingPageViewModel>(nameof(TestingPage));

		return builder;
    }

    // CommunityToolkit.Maui only has the overload with a view model
    static IServiceCollection AddTransientWithShellRoute<TView>(this IServiceCollection services, string route)
        where TView : NavigableElement
    {
        services.AddTransient<TView>();
        Routing.RegisterRoute(route, typeof(TView));
        return services;
    }

    static MauiAppBuilder InjectServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<ICalendarSettingsService, CalendarSettingsService>();
        return builder;
    }
}

