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

		base.OnCreate(savedInstanceState);
	}
}

