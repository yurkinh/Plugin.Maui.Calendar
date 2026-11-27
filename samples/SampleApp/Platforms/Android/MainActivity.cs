using Android.App;
using Android.Content.PM;
using Android.OS;
using SampleApp.Benchmarks;

namespace SampleApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		// Benchmark mode (benchmark.sh): adb shell am start ... --ez calbench true
		if (Intent?.GetBooleanExtra("calbench", false) == true)
		{
			BenchmarkLaunch.IsRequested = true;
		}

		base.OnCreate(savedInstanceState);
	}
}
