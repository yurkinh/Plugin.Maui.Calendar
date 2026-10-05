using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

namespace Plugin.Maui.Calendar.UITests.Tests;

/// <summary>Verifies selecting days by tapping them, in every kind of calendar.</summary>
public class SelectionTests : UITest
{
	[Test]
	public void Tap_SelectsTheDayAndTappingItAgainDeselectsIt()
	{
		App.OpenScenario("Default");

		App.TapDay(May(20));

		Assert.That(App.WaitForText("SelectedDate", "2025-05-20"), Is.EqualTo("2025-05-20"));
		Assert.That(App.Text("SelectedText"), Is.EqualTo("20 May 2025"));
		Assert.That(App.Text("DayTapped"), Is.EqualTo("1 2025-05-20"), "DayTappedCommand runs with the date");
		CalendarMatchesBaseline("Tap_SelectedDay");

		App.TapDay(May(20));

		Assert.That(App.WaitForText("SelectedDate", "none"), Is.EqualTo("none"));
	}

	[Test]
	public void Tap_AnotherDay_MovesTheSelection()
	{
		App.OpenScenario("SelectedDay");

		App.TapDay(May(8));

		Assert.That(App.WaitForText("SelectedDates", "2025-05-08"), Is.EqualTo("2025-05-08"));
	}

	[Test]
	public void Tap_OnADisabledDay_DoesNothing()
	{
		App.OpenScenario("DisabledDays");

		App.TapDay(May(12));
		App.TapDay(May(30));

		Assert.That(App.Text("SelectedDate"), Is.EqualTo("none"));
		Assert.That(App.Text("DayTapped"), Is.EqualTo("0"));
	}

	[Test]
	public void Tap_WithAllowDeselectingOff_KeepsTheSelectedDay()
	{
		App.OpenScenario("NoDeselect");

		App.TapDay(May(20));

		Assert.That(App.Text("SelectedDate"), Is.EqualTo("2025-05-20"));
	}

	[Test]
	public void MultiSelection_TapsAddAndRemoveDays()
	{
		App.OpenScenario("Multi");

		App.TapDay(May(6));
		App.TapDay(May(9));
		App.TapDay(May(21));
		Assert.That(App.WaitForText("SelectedDates", "2025-05-06,2025-05-09,2025-05-21"), Is.EqualTo("2025-05-06,2025-05-09,2025-05-21"));
		CalendarMatchesBaseline("Multi_ThreeDays");

		App.TapDay(May(9));

		Assert.That(App.WaitForText("SelectedDates", "2025-05-06,2025-05-21"), Is.EqualTo("2025-05-06,2025-05-21"));
	}

	[Test]
	public void RangeSelection_TwoTapsSelectTheRangeWithoutItsDisabledDays()
	{
		App.OpenScenario("Range");

		App.TapDay(May(12));
		App.TapDay(May(16));

		Assert.That(App.WaitForText("Range", "2025-05-12..2025-05-16"), Is.EqualTo("2025-05-12..2025-05-16"));
		Assert.That(App.Text("SelectedDates"), Is.EqualTo("2025-05-12,2025-05-13,2025-05-15,2025-05-16"), "May 14 is disabled");
		Assert.That(App.Text("SelectedText"), Is.EqualTo("12 May 2025 - 16 May 2025"));
		CalendarMatchesBaseline("Range_TwelveToSixteen");

		// The next tap starts a new range.
		App.TapDay(May(22));

		Assert.That(App.WaitForText("Range", "2025-05-22..2025-05-22"), Is.EqualTo("2025-05-22..2025-05-22"));
	}

	[Test]
	public void RangeSelection_SecondTapBeforeTheFirst_ExtendsTheRangeBackwards()
	{
		App.OpenScenario("Range");

		App.TapDay(May(20));
		App.TapDay(May(17));

		Assert.That(App.WaitForText("Range", "2025-05-17..2025-05-20"), Is.EqualTo("2025-05-17..2025-05-20"));
	}

	[Test]
	public void WeekSelection_TapSelectsTheWholeWeekAndTappingItAgainDeselectsIt()
	{
		App.OpenScenario("Week");

		App.TapDay(May(14));

		Assert.That(App.WaitForText("SelectedDates", "2025-05-12,2025-05-13,2025-05-14,2025-05-15,2025-05-16,2025-05-17,2025-05-18"),
			Is.EqualTo("2025-05-12,2025-05-13,2025-05-14,2025-05-15,2025-05-16,2025-05-17,2025-05-18"), "the week starts on Monday");
		CalendarMatchesBaseline("Week_MondayToSunday");

		App.TapDay(May(16));

		Assert.That(App.WaitForText("SelectedDates", "-"), Is.EqualTo("-"));
	}

	[Test]
	public void AutoChangeMonthOnDayTap_TapOnADayOfTheNextMonth_ShowsThatMonth()
	{
		App.OpenScenario("AutoChangeMonth");

		App.TapDay(June(2));

		Assert.That(App.WaitForText("ShownDate", "2025-06-02"), Is.EqualTo("2025-06-02"));
		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("June"));
		Assert.That(App.Text("SelectedDate"), Is.EqualTo("2025-06-02"));
		Assert.That(App.Text("MonthChanged"), Is.EqualTo("1 2025-05-14>2025-06-02"));
	}

	[Test]
	public void DayTemplate_TappingATemplatedDay_SelectsIt()
	{
		App.OpenScenario("DayTemplate");

		App.TapDay(May(9));

		Assert.That(App.WaitForText("SelectedDate", "2025-05-09"), Is.EqualTo("2025-05-09"));
		CalendarMatchesBaseline("DayTemplate_Tapped");
	}

	[Test]
	public void EventsList_ShowsTheEventsOfTheTappedDay()
	{
		App.OpenScenario("EventsList");

		App.TapDay(May(20));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-20"), Is.EqualTo("2025-05-20"));
		CalendarMatchesBaseline("EventsList_ThreeEvents");

		App.TapDay(May(21));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-21"), Is.EqualTo("2025-05-21"));
		CalendarMatchesBaseline("EventsList_NoEvents");
	}
}
