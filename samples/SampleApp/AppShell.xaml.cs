using SampleApp.Views;

namespace SampleApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(UserSettingPage), typeof(UserSettingPage));
		Routing.RegisterRoute(nameof(SimplePage), typeof(SimplePage));
		Routing.RegisterRoute(nameof(WeekendCalendarPage), typeof(WeekendCalendarPage));
		Routing.RegisterRoute(nameof(MultiSelectionPage), typeof(MultiSelectionPage));
		Routing.RegisterRoute(nameof(AdvancedPage), typeof(AdvancedPage));
		Routing.RegisterRoute(nameof(RangeSelectionPage), typeof(RangeSelectionPage));
		Routing.RegisterRoute(nameof(WeekViewPage), typeof(WeekViewPage));
		Routing.RegisterRoute(nameof(TwoWeekViewPage), typeof(TwoWeekViewPage));
		Routing.RegisterRoute(nameof(Windows11CalendarPage), typeof(Windows11CalendarPage));
		Routing.RegisterRoute(nameof(XiaomiCalendarPage), typeof(XiaomiCalendarPage));
		Routing.RegisterRoute(nameof(EditEventPage), typeof(EditEventPage));
	}
}