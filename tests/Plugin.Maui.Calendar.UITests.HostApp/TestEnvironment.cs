using System.Globalization;

namespace Plugin.Maui.Calendar.UITests.HostApp;

/// <summary>
/// The fixed environment of every scenario. The UI tests compare screenshots and texts with expected
/// ones, so nothing may depend on the day the tests run, the device's language or its clock.
/// </summary>
public static class TestEnvironment
{
	/// <summary>The day every calendar considers today: a Wednesday in the middle of May 2025.</summary>
	public static readonly DateTime Today = new(2025, 5, 14);

	public static readonly CultureInfo Culture = new("en-US");

	public static readonly TimeProvider TimeProvider = new FixedTimeProvider(Today.AddHours(10));

	sealed class FixedTimeProvider(DateTime localNow) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => new(localNow, TimeSpan.Zero);

		public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
	}
}
