using System.Reflection;

namespace Plugin.Maui.Calendar.UITests.Infrastructure;

/// <summary>
/// Where and how the UI tests run, read from environment variables (all optional):
/// <list type="bullet">
/// <item><c>UITEST_PLATFORM</c>: <c>ios</c>, <c>android</c>, <c>maccatalyst</c> or <c>windows</c>; by default
/// <c>ios</c> on macOS, <c>windows</c> on Windows and <c>android</c> elsewhere.</item>
/// <item><c>UITEST_CONFIGURATION</c>: the configuration the host app was built with (<c>Debug</c> by default).</item>
/// <item><c>UITEST_APP</c>: the app to test, instead of the host app's build output.</item>
/// <item><c>UITEST_DEVICE</c>: the iOS simulator (<c>iPhone 17</c> by default) or the Android emulator (AVD) to use.</item>
/// <item><c>UITEST_PLATFORM_VERSION</c>: the iOS or Android version of that device.</item>
/// <item><c>UITEST_APPIUM_URL</c>: an Appium server that is already running; otherwise one is started from this folder.</item>
/// <item><c>UITEST_UPDATE_BASELINES</c>: <c>1</c> to save the screenshots as the new baselines instead of comparing them.</item>
/// <item><c>UITEST_SCREENSHOTS</c>: <c>off</c> to skip the screenshot comparisons and check the behavior only, on a
/// device the baselines were not recorded on (fonts and rendering differ between devices and OS versions).</item>
/// </list>
/// </summary>
public static class UITestSettings
{
	public const string AppId = "com.plugin.maui.calendar.uitests";

	public static TestPlatform Platform { get; } = ReadPlatform();

	public static string Configuration { get; } = Environment.GetEnvironmentVariable("UITEST_CONFIGURATION") is { Length: > 0 } configuration ? configuration : "Debug";

	public static string? Device { get; } = Environment.GetEnvironmentVariable("UITEST_DEVICE") is { Length: > 0 } device ? device : null;

	public static string? PlatformVersion { get; } = Environment.GetEnvironmentVariable("UITEST_PLATFORM_VERSION") is { Length: > 0 } version ? version : null;

	public static Uri? AppiumUrl { get; } = Environment.GetEnvironmentVariable("UITEST_APPIUM_URL") is { Length: > 0 } url ? new Uri(url) : null;

	public static bool UpdateBaselines { get; } = Environment.GetEnvironmentVariable("UITEST_UPDATE_BASELINES") is "1" or "true";

	public static bool CompareScreenshots { get; } = Environment.GetEnvironmentVariable("UITEST_SCREENSHOTS") is not ("off" or "0" or "false");

	/// <summary>This project's folder (it holds the baselines and Appium's node_modules).</summary>
	public static string ProjectDirectory { get; } = typeof(UITestSettings).Assembly
		.GetCustomAttributes<AssemblyMetadataAttribute>()
		.Single(attribute => attribute.Key == "ProjectDirectory").Value!;

	public static string BaselineDirectory => Path.Combine(ProjectDirectory, "Baselines", Platform.ToString());

	/// <summary>Where the screenshots of failed comparisons are written.</summary>
	public static string ResultsDirectory => Path.Combine(ProjectDirectory, "TestResults", "Screenshots", Platform.ToString());

	/// <summary>The app to install and start.</summary>
	public static string AppPath => Environment.GetEnvironmentVariable("UITEST_APP") is { Length: > 0 } app ? app : FindHostApp();

	static TestPlatform ReadPlatform() => Environment.GetEnvironmentVariable("UITEST_PLATFORM")?.ToLowerInvariant() switch
	{
		"ios" => TestPlatform.iOS,
		"android" => TestPlatform.Android,
		"maccatalyst" or "mac" => TestPlatform.MacCatalyst,
		"windows" => TestPlatform.Windows,
		null or "" when OperatingSystem.IsMacOS() => TestPlatform.iOS,
		null or "" when OperatingSystem.IsWindows() => TestPlatform.Windows,
		null or "" => TestPlatform.Android,
		var other => throw new InvalidOperationException($"Unknown UITEST_PLATFORM '{other}'. Use ios, android, maccatalyst or windows."),
	};

	static string FindHostApp()
	{
		var bin = Path.GetFullPath(Path.Combine(ProjectDirectory, "..", "Plugin.Maui.Calendar.UITests.HostApp", "bin", Configuration));
		var (framework, pattern) = Platform switch
		{
			TestPlatform.iOS => ("net10.0-ios", "*.app"),
			TestPlatform.MacCatalyst => ("net10.0-maccatalyst", "*.app"),
			TestPlatform.Android => ("net10.0-android", $"{AppId}-Signed.apk"),
			_ => ("net10.0-windows10.0.19041.0", "Plugin.Maui.Calendar.UITests.HostApp.exe"),
		};

		var directory = Path.Combine(bin, framework);
		var app = Directory.Exists(directory)
			? Directory.EnumerateFileSystemEntries(directory, pattern, SearchOption.AllDirectories)
				// The iOS build output also holds an .app for the device; the tests use the simulator's.
				.Where(path => Platform != TestPlatform.iOS || path.Contains("iossimulator", StringComparison.Ordinal))
				// The bundle itself, not a bundle nested in it.
				.OrderBy(path => path.Length)
				.FirstOrDefault()
			: null;

		return app ?? throw new FileNotFoundException(
			$"The host app was not found in {directory}. Build it first, for example: " +
			$"dotnet build tests/Plugin.Maui.Calendar.UITests.HostApp -f {framework} -c {Configuration}");
	}
}
