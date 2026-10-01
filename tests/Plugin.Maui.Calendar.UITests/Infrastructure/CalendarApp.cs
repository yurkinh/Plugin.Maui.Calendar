using System.Drawing;
using System.Globalization;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Plugin.Maui.Calendar.UITests.Infrastructure;

/// <summary>A direction to swipe over an element in.</summary>
public enum Swipe
{
	Left,
	Right,
	Up,
	Down,
}

/// <summary>
/// The host app driven through Appium. Elements are found by their automation id (see ScenarioPage in the
/// host app for the ids of the calendar's own elements and of the state texts).
/// </summary>
public sealed class CalendarApp : IDisposable
{
	static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(15);

	readonly AppiumDriver driver;

	CalendarApp(AppiumDriver driver)
	{
		this.driver = driver;
	}

	public static CalendarApp Launch(Uri server)
	{
		var options = new AppiumOptions { App = UITestSettings.AppPath };
		if (UITestSettings.PlatformVersion is { } platformVersion)
		{
			options.PlatformVersion = platformVersion;
		}

		options.AddAdditionalAppiumOption("newCommandTimeout", 600);

		// Building and starting the driver's agent on the device can take minutes the first time.
		var sessionTimeout = TimeSpan.FromMinutes(10);

		AppiumDriver driver;
		switch (UITestSettings.Platform)
		{
			case TestPlatform.iOS:
				options.PlatformName = "iOS";
				options.AutomationName = "XCUITest";
				options.DeviceName = UITestSettings.Device ?? "iPhone 17";
				options.AddAdditionalAppiumOption("wdaLaunchTimeout", 600_000);
				options.AddAdditionalAppiumOption("wdaConnectionTimeout", 600_000);
				options.AddAdditionalAppiumOption("simulatorStartupTimeout", 300_000);
				options.AddAdditionalAppiumOption("reduceMotion", true);
				options.AddAdditionalAppiumOption("shouldTerminateApp", true);
				driver = new IOSDriver(server, options, sessionTimeout);
				break;

			case TestPlatform.Android:
				options.PlatformName = "Android";
				options.AutomationName = "UiAutomator2";
				options.DeviceName = "Android Emulator";
				if (UITestSettings.Device is { } avd)
				{
					options.AddAdditionalAppiumOption("avd", avd);
					options.AddAdditionalAppiumOption("avdLaunchTimeout", 300_000);
					options.AddAdditionalAppiumOption("avdReadyTimeout", 300_000);
				}

				options.AddAdditionalAppiumOption("disableWindowAnimation", true);

				// Every build has the same version code, so the app is installed again rather than kept.
				options.AddAdditionalAppiumOption("enforceAppInstall", true);
				options.AddAdditionalAppiumOption("uiautomator2ServerInstallTimeout", 120_000);
				options.AddAdditionalAppiumOption("adbExecTimeout", 120_000);
				driver = new AndroidDriver(server, options, sessionTimeout);
				break;

			case TestPlatform.MacCatalyst:
				options.App = null;
				options.PlatformName = "mac";
				options.AutomationName = "Mac2";
				options.AddAdditionalAppiumOption("bundleId", UITestSettings.AppId);
				options.AddAdditionalAppiumOption("appPath", UITestSettings.AppPath);
				options.AddAdditionalAppiumOption("showServerLogs", true);
				driver = new MacDriver(server, options, sessionTimeout);
				break;

			default:
				options.PlatformName = "Windows";
				options.AutomationName = "Windows";
				options.DeviceName = "WindowsPC";
				driver = new WindowsDriver(server, options, sessionTimeout);
				break;
		}

		driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
		return new CalendarApp(driver);
	}

	// ── Elements ─────────────────────────────────────────────────────────────

	/// <summary>The element with <paramref name="automationId"/>, waiting until it exists.</summary>
	public IWebElement Element(string automationId, TimeSpan? timeout = null) =>
		Wait(timeout).Until(driver => driver.FindElements(ByAutomationId(automationId)).FirstOrDefault())
			?? throw new NoSuchElementException(automationId);

	public bool Exists(string automationId) =>
		driver.FindElements(ByAutomationId(automationId)).Count > 0;

	// MAUI makes the automation id the view's resource id on Android, and its accessibility identifier elsewhere.
	static By ByAutomationId(string automationId) => UITestSettings.Platform == TestPlatform.Android
		? By.Id(automationId)
		: MobileBy.AccessibilityId(automationId);

	/// <summary>The text of the label with <paramref name="automationId"/>.</summary>
	public string Text(string automationId)
	{
		var element = Element(automationId);

		// A Mac Catalyst label exposes its text as its value.
		return UITestSettings.Platform == TestPlatform.MacCatalyst
			? element.GetAttribute("value") ?? element.Text
			: element.Text;
	}

	/// <summary>Waits until the label with <paramref name="automationId"/> shows <paramref name="expected"/>, and returns what it shows.</summary>
	public string WaitForText(string automationId, string expected, TimeSpan? timeout = null)
	{
		var text = string.Empty;
		try
		{
			Wait(timeout ?? TimeSpan.FromSeconds(5)).Until(_ => (text = Text(automationId)) == expected);
		}
		catch (WebDriverTimeoutException)
		{
			// The caller asserts on the returned text, which gives the better message.
		}

		return text;
	}

	public void Tap(string automationId) => Element(automationId).Click();

	public bool IsEnabled(string automationId) => Element(automationId).Enabled;

	// ── Scenarios ────────────────────────────────────────────────────────────

	/// <summary>Opens the scenario of the host app named <paramref name="name"/> (see ScenarioCatalog).</summary>
	public void OpenScenario(string name)
	{
		if (!Exists("ScenarioEntry"))
		{
			if (!Exists("BackButton"))
			{
				Restart();
			}
			else
			{
				Tap("BackButton");
			}
		}

		var entry = Element("ScenarioEntry");
		entry.Click();
		entry.Clear();
		entry.SendKeys(name);
		Tap("GoButton");

		var title = WaitForText("ScenarioTitle", name, defaultTimeout);
		if (title != name)
		{
			throw new InvalidOperationException($"The scenario '{name}' did not open (the title shows '{title}').");
		}

		Element("Calendar");
	}

	void Restart()
	{
		driver.TerminateApp(UITestSettings.AppId);
		driver.ActivateApp(UITestSettings.AppId);
	}

	// ── Days ─────────────────────────────────────────────────────────────────

	/// <summary>The first date the calendar shows (its first cell, <c>Day0</c>).</summary>
	public DateTime VisibleStartDate =>
		DateTime.ParseExact(Text("VisibleDates")[..10], "yyyy-MM-dd", CultureInfo.InvariantCulture);

	/// <summary>The automation id of the cell that shows <paramref name="date"/>.</summary>
	public string DayCell(DateTime date)
	{
		var index = (date.Date - VisibleStartDate).Days;
		if (index is < 0 or > 41)
		{
			throw new ArgumentOutOfRangeException(nameof(date), date, "The calendar does not show this date.");
		}

		return $"Day{index}";
	}

	public void TapDay(DateTime date) => Tap(DayCell(date));

	// ── Gestures ─────────────────────────────────────────────────────────────

	/// <summary>Swipes over the middle of the element with <paramref name="automationId"/>.</summary>
	public void SwipeOver(string automationId, Swipe direction)
	{
		var element = Element(automationId);
		var rect = new Rectangle(element.Location, element.Size);
		var center = new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
		var (from, to) = direction switch
		{
			Swipe.Left => (new Point(rect.X + (rect.Width * 4 / 5), center.Y), new Point(rect.X + (rect.Width / 5), center.Y)),
			Swipe.Right => (new Point(rect.X + (rect.Width / 5), center.Y), new Point(rect.X + (rect.Width * 4 / 5), center.Y)),
			Swipe.Up => (new Point(center.X, rect.Y + (rect.Height * 4 / 5)), new Point(center.X, rect.Y + (rect.Height / 5))),
			_ => (new Point(center.X, rect.Y + (rect.Height / 5)), new Point(center.X, rect.Y + (rect.Height * 4 / 5))),
		};

		var isTouch = UITestSettings.Platform is TestPlatform.iOS or TestPlatform.Android;
		var pointer = new PointerInputDevice(isTouch ? PointerKind.Touch : PointerKind.Mouse, "finger");
		var button = isTouch ? MouseButton.Touch : MouseButton.Left;
		var swipe = new ActionSequence(pointer, 0);
		swipe.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, from.X, from.Y, TimeSpan.Zero));
		swipe.AddAction(pointer.CreatePointerDown(button));
		swipe.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, to.X, to.Y, TimeSpan.FromMilliseconds(200)));
		swipe.AddAction(pointer.CreatePointerUp(button));
		driver.PerformActions([swipe]);
	}

	// ── Screenshots ──────────────────────────────────────────────────────────

	/// <summary>
	/// A PNG screenshot of the element with <paramref name="automationId"/>, taken once two screenshots in a
	/// row are the same, so layout passes and animations that are still running never end up in it.
	/// </summary>
	public byte[] Screenshot(string automationId)
	{
		var element = Element(automationId);
		var previous = ((ITakesScreenshot)element).GetScreenshot().AsByteArray;

		for (var attempt = 0; attempt < 10; attempt++)
		{
			Thread.Sleep(300);
			var current = ((ITakesScreenshot)element).GetScreenshot().AsByteArray;
			if (current.AsSpan().SequenceEqual(previous))
			{
				return current;
			}

			previous = current;
		}

		return previous;
	}

	/// <summary>Saves the screen and the element tree, to see what the app showed when a test failed.</summary>
	public IReadOnlyList<string> SaveDiagnostics(string name)
	{
		Directory.CreateDirectory(UITestSettings.ResultsDirectory);
		var screen = Path.Combine(UITestSettings.ResultsDirectory, name + ".screen.png");
		var source = Path.Combine(UITestSettings.ResultsDirectory, name + ".source.xml");
		driver.GetScreenshot().SaveAsFile(screen);
		File.WriteAllText(source, driver.PageSource);
		return [screen, source];
	}

	WebDriverWait Wait(TimeSpan? timeout) => new(driver, timeout ?? defaultTimeout)
	{
		PollingInterval = TimeSpan.FromMilliseconds(250),
	};

	public void Dispose()
	{
		driver.Quit();
		driver.Dispose();
	}
}
