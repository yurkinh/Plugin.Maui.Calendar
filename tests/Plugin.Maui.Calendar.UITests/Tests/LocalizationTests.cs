using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

namespace Plugin.Maui.Calendar.UITests.Tests;

/// <summary>Verifies the texts the calendar writes in other cultures.</summary>
public class LocalizationTests : UITest
{
	[Test]
	public void Ukrainian_MonthNameAndSelectedDate()
	{
		App.OpenScenario("Ukrainian");

		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("Травень"));
		Assert.That(App.Text("SelectedText"), Does.StartWith("20 трав"));
		Assert.That(App.Text("VisibleDates"), Is.EqualTo("2025-04-28..2025-06-08"), "the week starts on Monday");
	}

	[Test]
	public void Persian_ShowsTheGregorianMonthOfTheDays()
	{
		// The Persian calendar's month would be "اردیبهشت"; the days shown are the Gregorian May.
		App.OpenScenario("Persian");

		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("مه"));
		Assert.That(App.Text("SelectedText"), Is.EqualTo("20 مه 2025"));
	}

	[Test]
	public void ArabicWithNativeDigits_WritesNumbersInArabicIndicDigits()
	{
		App.OpenScenario("ArabicNativeDigits");

		Assert.That(App.Text("SelectedText"), Is.EqualTo("٢٠ مايو ٢٠٢٥"));
		Assert.That(App.Text("YearLabel"), Is.EqualTo("٢٠٢٥"));
	}

	[Test]
	public void RightToLeft_TapSelectsTheDayUnderTheFinger()
	{
		// The grid runs from right to left (FlowDirection), Saturday first: the tap lands on the day
		// that is shown there.
		App.OpenScenario("ArabicNativeDigits");

		App.TapDay(May(12));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-12"), Is.EqualTo("2025-05-12"));

		App.TapDay(May(16));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-16"), Is.EqualTo("2025-05-16"));
	}
}
