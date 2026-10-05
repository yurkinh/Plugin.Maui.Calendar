using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

namespace Plugin.Maui.Calendar.UITests.Tests;

/// <summary>Verifies the swipes over the days and showing and hiding the calendar section.</summary>
public class GestureTests : UITest
{
	[Test]
	public void SwipeLeftAndRight_ShowTheNextAndPreviousMonth()
	{
		App.OpenScenario("Default");

		App.SwipeOver("DaysGrid", Swipe.Left);

		Assert.That(App.WaitForText("LayoutUnit", "June"), Is.EqualTo("June"));
		Assert.That(App.Text("Swiped"), Is.EqualTo("L1 R0 U0 D0"));
		Assert.That(App.Text("MonthChanged"), Is.EqualTo("1 2025-05-14>2025-06-14"));

		App.SwipeOver("DaysGrid", Swipe.Right);
		App.SwipeOver("DaysGrid", Swipe.Right);

		Assert.That(App.WaitForText("LayoutUnit", "April"), Is.EqualTo("April"));
		Assert.That(App.Text("Swiped"), Is.EqualTo("L1 R2 U0 D0"));
	}

	[Test]
	public void SwipeUp_HidesTheCalendarSectionAndTheFooterArrowShowsItAgain()
	{
		App.OpenScenario("Default");

		App.SwipeOver("DaysGrid", Swipe.Up);

		Assert.That(App.WaitForText("SectionShown", "False"), Is.EqualTo("False"));
		CalendarMatchesBaseline("SectionCollapsed");

		App.Tap("FooterArrow");

		Assert.That(App.WaitForText("SectionShown", "True"), Is.EqualTo("True"));
		CalendarMatchesBaseline("Default");
	}

	[Test]
	public void SwipeDown_IsReportedAndChangesNothing()
	{
		App.OpenScenario("Default");

		App.SwipeOver("DaysGrid", Swipe.Down);

		Assert.That(App.WaitForText("Swiped", "L0 R0 U0 D1"), Is.EqualTo("L0 R0 U0 D1"));
		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("May"));
		Assert.That(App.Text("SectionShown"), Is.EqualTo("True"));
	}

	[Test]
	public void Swipes_RespectTheArrowLimits()
	{
		App.OpenScenario("Limits");

		App.SwipeOver("DaysGrid", Swipe.Right);

		Assert.That(App.WaitForText("Swiped", "L0 R1 U0 D0"), Is.EqualTo("L0 R1 U0 D0"));
		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("May"), "April has no allowed day");
	}

	[Test]
	public void SwipeDetectionDisabled_SwipesDoNothing()
	{
		App.OpenScenario("SwipeDisabled");

		App.SwipeOver("DaysGrid", Swipe.Left);
		App.SwipeOver("DaysGrid", Swipe.Up);

		Thread.Sleep(500);
		Assert.That(App.Text("Swiped"), Is.EqualTo("L0 R0 U0 D0"));
		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("May"));
		Assert.That(App.Text("SectionShown"), Is.EqualTo("True"));
	}

	[Test]
	public void SwipeActionsTurnedOff_SwipesAreReportedButChangeNothing()
	{
		App.OpenScenario("SwipeToChangeMonthOff");

		App.SwipeOver("DaysGrid", Swipe.Left);
		App.SwipeOver("DaysGrid", Swipe.Up);

		Assert.That(App.WaitForText("Swiped", "L1 R0 U1 D0"), Is.EqualTo("L1 R0 U1 D0"));
		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("May"));
		Assert.That(App.Text("SectionShown"), Is.EqualTo("True"));
	}

	[Test]
	public void SectionHiddenFromTheStart_CanBeShown()
	{
		// The section used to stay hidden for good: it had never been laid out, so its height was unknown.
		App.OpenScenario("SectionHidden");
		Assert.That(App.Text("SectionShown"), Is.EqualTo("False"));

		App.Tap("FooterArrow");

		Assert.That(App.WaitForText("SectionShown", "True"), Is.EqualTo("True"));
		CalendarMatchesBaseline("Default");
	}
}
