using System.Diagnostics;

namespace SampleApp.Benchmarks;

/// <summary>
/// Benchmark mode: the app opens the benchmark page right after start, runs it and writes the results to the
/// console. benchmark.sh starts the app this way: on Android with the intent extra <c>calbench</c>
/// (<c>adb shell am start ... --ez calbench true</c>), on iOS and Mac Catalyst with the environment variable
/// <c>CALBENCH=1</c> (<c>SIMCTL_CHILD_CALBENCH=1 xcrun simctl launch ...</c>).
/// </summary>
public static class BenchmarkLaunch
{
	public static bool IsRequested { get; set; } = Environment.GetEnvironmentVariable("CALBENCH") == "1";

	/// <summary>
	/// Time from the start of the process until the first page appeared, or null where the platform does not
	/// report the process start time.
	/// </summary>
	public static double? StartupMilliseconds { get; private set; }

	static bool firstPageAppeared;

	/// <summary>Called when the first page appears; opens the benchmark page in benchmark mode.</summary>
	public static void OnFirstPageAppeared()
	{
		if (firstPageAppeared)
		{
			return;
		}

		firstPageAppeared = true;
		StartupMilliseconds = MeasureStartup();

		if (IsRequested)
		{
			MainThread.BeginInvokeOnMainThread(() => Shell.Current.GoToAsync(nameof(Views.BenchmarkPage)));
		}
	}

	static double? MeasureStartup()
	{
		// iOS and Mac Catalyst do not report the process start time
		if (OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
		{
			return null;
		}

		try
		{
			var startTime = Process.GetCurrentProcess().StartTime;
			return (DateTime.Now - startTime).TotalMilliseconds;
		}
		catch (Exception exception) when (exception is PlatformNotSupportedException or InvalidOperationException or NotSupportedException)
		{
			return null;
		}
	}
}
