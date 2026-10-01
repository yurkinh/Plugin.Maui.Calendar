using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

// In the root namespace, so it applies to the tests of every namespace below it.
namespace Plugin.Maui.Calendar.UITests;

/// <summary>Starts Appium and the app once for the whole run; every test then opens its own scenario.</summary>
[SetUpFixture]
public sealed class UITestSetup
{
	static AppiumServer? server;
	static CalendarApp? app;

	public static CalendarApp App => app ?? throw new InvalidOperationException("The app was not started.");

	[OneTimeSetUp]
	public void StartApp()
	{
		var url = UITestSettings.AppiumUrl;
		if (url is null)
		{
			server = AppiumServer.Start();
			url = server.Url;
		}

		app = CalendarApp.Launch(url);
	}

	[OneTimeTearDown]
	public void StopApp()
	{
		app?.Dispose();
		server?.Dispose();
	}
}
