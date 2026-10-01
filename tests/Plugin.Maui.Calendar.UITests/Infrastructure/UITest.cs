using NUnit.Framework;

namespace Plugin.Maui.Calendar.UITests.Infrastructure;

/// <summary>
/// The base of every UI test. The scenarios show May 2025 and today is May 14 2025, a Wednesday; with Sunday
/// as the first day of the week the month grid runs from April 27 to June 7.
/// </summary>
public abstract class UITest
{
	protected static readonly DateTime Today = new(2025, 5, 14);

	protected static CalendarApp App => UITestSetup.App;

	protected static TestPlatform Platform => UITestSettings.Platform;

	protected static DateTime May(int day) => new(2025, 5, day);

	protected static DateTime June(int day) => new(2025, 6, day);

	protected static string Format(DateTime date) => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

	[TearDown]
	public void SaveDiagnosticsOfFailure()
	{
		if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed)
		{
			return;
		}

		try
		{
			var name = string.Concat(TestContext.CurrentContext.Test.Name.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
			foreach (var path in App.SaveDiagnostics(name))
			{
				TestContext.AddTestAttachment(path);
			}
		}
		catch (Exception exception) when (exception is OpenQA.Selenium.WebDriverException or InvalidOperationException or IOException)
		{
			// The app or the session is gone; the test's own failure says enough.
		}
	}

	/// <summary>Compares a screenshot of the calendar with the baseline named <paramref name="name"/>.</summary>
	protected static void CalendarMatchesBaseline(string name)
	{
		if (UITestSettings.CompareScreenshots)
		{
			ScreenshotAssert.MatchesBaseline(App.Screenshot("Calendar"), name);
		}
	}

	/// <summary>Skips a test on platforms that cannot do what it needs.</summary>
	protected static void SkipOn(TestPlatform platform, string reason)
	{
		if (Platform == platform)
		{
			Assert.Ignore($"Not supported on {platform}: {reason}");
		}
	}
}
