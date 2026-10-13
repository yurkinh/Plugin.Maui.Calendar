using Android.App;
using Android.Content.PM;

namespace SampleApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Android.OS.Bundle savedInstanceState)
	{
		// adb shell am start -n <activity> --ez calbench true opens the DrawnUI spike benchmark.
		if (Intent?.GetBooleanExtra("calbench", false) == true)
		{
			Views.DrawnSpikeBenchmarkPage.Requested = true;
		}

		// --ei calparity <n> opens scenario n of the DrawnUI parity page.
		if (Intent?.GetIntExtra("calparity", -1) is >= 0 and var scenario)
		{
			Views.DrawnParityPage.Scenario = scenario;
		}

		// --es calroute <route> opens that sample page at startup (see AppShell).
		if (Intent?.GetStringExtra("calroute") is { Length: > 0 } route)
		{
			AppShell.StartupRoute = route;
		}

		// --ez caldrawn true draws the day grids of every page with DrawnUI (see App).
		if (Intent?.GetBooleanExtra("caldrawn", false) == true)
		{
			AppContext.SetSwitch("Plugin.Maui.Calendar.DrawnDayGrid", true);
		}

		// --ez calgraphics true draws them with Microsoft.Maui.Graphics instead: the grid below on the
		// parity page, otherwise the day grids of every page.
		if (Intent?.GetBooleanExtra("calgraphics", false) == true)
		{
			Views.DrawnParityPage.Graphics = true;
			if (Views.DrawnParityPage.Scenario < 0)
			{
				AppContext.SetSwitch("Plugin.Maui.Calendar.GraphicsDayGrid", true);
			}
		}

		base.OnCreate(savedInstanceState);
	}
}

