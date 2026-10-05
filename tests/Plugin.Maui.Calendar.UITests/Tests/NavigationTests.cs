using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

namespace Plugin.Maui.Calendar.UITests.Tests;

/// <summary>Verifies the month and year arrows, their limits and the custom header's own arrows.</summary>
public class NavigationTests : UITest
{
	[Test]
	public void MonthArrows_ShowTheNextAndPreviousMonthAndRaiseMonthChanged()
	{
		App.OpenScenario("Default");

		App.Tap("NextMonthArrow");

		Assert.That(App.WaitForText("LayoutUnit", "June"), Is.EqualTo("June"));
		Assert.That(App.Text("ShownDate"), Is.EqualTo("2025-06-14"));
		Assert.That(App.Text("VisibleDates"), Is.EqualTo("2025-06-01..2025-07-12"));
		Assert.That(App.Text("MonthChanged"), Is.EqualTo("1 2025-05-14>2025-06-14"));
		CalendarMatchesBaseline("June");

		App.Tap("PrevMonthArrow");
		App.Tap("PrevMonthArrow");

		Assert.That(App.WaitForText("LayoutUnit", "April"), Is.EqualTo("April"));
		Assert.That(App.Text("VisibleDates"), Is.EqualTo("2025-03-30..2025-05-10"));
		Assert.That(App.Text("MonthChanged"), Is.EqualTo("3 2025-05-14>2025-04-14"));
	}

	[Test]
	public void YearArrows_ShowTheSameMonthOfTheNextAndPreviousYear()
	{
		App.OpenScenario("Default");

		App.Tap("NextYearArrow");

		Assert.That(App.WaitForText("ShownDate", "2026-05-14"), Is.EqualTo("2026-05-14"));
		Assert.That(App.Text("YearLabel"), Is.EqualTo("2026"));
		Assert.That(App.Text("MonthChanged"), Is.EqualTo("1 2025-05-14>2026-05-14"));

		App.Tap("PrevYearArrow");
		App.Tap("PrevYearArrow");

		Assert.That(App.WaitForText("ShownDate", "2024-05-14"), Is.EqualTo("2024-05-14"));
		Assert.That(App.Text("YearLabel"), Is.EqualTo("2024"));
	}

	[Test]
	public void Arrows_AreDisabledWhereTheyWouldLeaveTheAllowedDates()
	{
		// MinimumDate is May 6 2025 and MaximumDate June 20 2025.
		App.OpenScenario("Limits");

		Assert.That(App.Text("Arrows"), Is.EqualTo("False,True,False,False"));
		Assert.That(App.IsEnabled("PrevMonthArrow"), Is.False, "April has no allowed day");
		Assert.That(App.IsEnabled("NextMonthArrow"), Is.True);
		Assert.That(App.IsEnabled("PrevYearArrow"), Is.False);
		Assert.That(App.IsEnabled("NextYearArrow"), Is.False);
		CalendarMatchesBaseline("Limits_May");

		App.Tap("NextMonthArrow");

		Assert.That(App.WaitForText("Arrows", "True,False,False,False"), Is.EqualTo("True,False,False,False"));
		Assert.That(App.IsEnabled("NextMonthArrow"), Is.False, "July has no allowed day");

		// A disabled arrow does nothing.
		App.Tap("NextMonthArrow");
		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("June"));
	}

	[Test]
	public void WeekLayoutArrows_MoveByAWeek()
	{
		App.OpenScenario("WeekLayout");
		Assert.That(App.Text("VisibleDates"), Is.EqualTo("2025-05-11..2025-05-17"));

		App.Tap("NextMonthArrow");

		Assert.That(App.WaitForText("VisibleDates", "2025-05-18..2025-05-24"), Is.EqualTo("2025-05-18..2025-05-24"));
		Assert.That(App.Text("MonthChanged"), Is.EqualTo("1 2025-05-14>2025-05-21"));

		App.Tap("PrevMonthArrow");
		App.Tap("PrevMonthArrow");

		Assert.That(App.WaitForText("VisibleDates", "2025-05-04..2025-05-10"), Is.EqualTo("2025-05-04..2025-05-10"));
	}

	[Test]
	public void TwoWeekLayoutArrows_MoveByTwoWeeks()
	{
		App.OpenScenario("TwoWeekLayout");
		Assert.That(App.Text("VisibleDates"), Is.EqualTo("2025-05-11..2025-05-24"));

		App.Tap("NextMonthArrow");

		Assert.That(App.WaitForText("VisibleDates", "2025-05-25..2025-06-07"), Is.EqualTo("2025-05-25..2025-06-07"));
	}

	[Test]
	public void WeekNumberHeader_FollowsTheShownWeek()
	{
		App.OpenScenario("WeekNumber");
		Assert.That(App.Text("LayoutUnit"), Is.EqualTo("20"));

		App.Tap("NextMonthArrow");

		Assert.That(App.WaitForText("LayoutUnit", "21"), Is.EqualTo("21"));
	}

	[Test]
	public void CustomHeader_ItsOwnArrowsUseTheCalendarsCommands()
	{
		App.OpenScenario("CustomHeaderFooter");
		Assert.That(App.Text("CustomTitle"), Is.EqualTo("May"));

		App.Tap("CustomNext");

		Assert.That(App.WaitForText("CustomTitle", "June"), Is.EqualTo("June"));
		Assert.That(App.Text("ShownDate"), Is.EqualTo("2025-06-14"));

		App.Tap("CustomPrev");

		Assert.That(App.WaitForText("CustomTitle", "May"), Is.EqualTo("May"));
	}
}
