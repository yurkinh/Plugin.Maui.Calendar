using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

namespace Plugin.Maui.Calendar.UITests.Tests;

/// <summary>
/// Verifies what a screen reader finds in the day grid drawn with Microsoft.Maui.Graphics (UITEST_DRAWN=graphics):
/// every shown day is a button named with its full date, selected while the day is selected and unavailable
/// while it is disabled, and the screen reader's activation taps it. The native cells have no such names, and
/// the DrawnUI grid shows its elements only while TalkBack or VoiceOver runs, so the tests run with this grid only.
/// </summary>
public class AccessibilityTests : UITest
{
	[SetUp]
	public void OnlyForTheGraphicsGrid() =>
		Assume.That(UITestSettings.GridMode, Is.EqualTo("graphics"), "Only the Microsoft.Maui.Graphics grid names its days for screen readers.");

	[Test]
	public void Days_AreButtonsNamedWithTheirFullDate_SelectedAndDisabledAsTheDay()
	{
		App.OpenScenario("DisabledDays");

		var day = App.ElementNamed("Friday, May 16, 2025");
		Assert.That(day.GetAttribute("enabled"), Is.EqualTo("true"));
		Assert.That(day.GetAttribute("selected"), Is.Not.EqualTo("true"));

		day.Click();

		Assert.That(App.WaitForText("SelectedDate", "2025-05-16"), Is.EqualTo("2025-05-16"));
		Assert.That(App.ElementNamed("Friday, May 16, 2025").GetAttribute("selected"), Is.EqualTo("true"));
		Assert.That(App.ElementNamed("Monday, May 12, 2025").GetAttribute("enabled"), Is.EqualTo("false"));
	}

	[Test]
	public void Days_FollowTheShownMonth()
	{
		App.OpenScenario("Default");

		App.Tap("NextMonthArrow");

		Assert.That(App.WaitForText("LayoutUnit", "June"), Is.EqualTo("June"));
		Assert.That(App.ElementNamed("Monday, June 16, 2025").GetAttribute("enabled"), Is.EqualTo("true"));
	}
}
