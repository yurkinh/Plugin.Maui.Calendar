using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

namespace Plugin.Maui.Calendar.UITests.Tests;

/// <summary>
/// Verifies that the calendar follows properties changed while it is on screen (the action buttons of the
/// Interactive scenario), and that it keeps working afterwards.
/// </summary>
public class RuntimeChangeTests : UITest
{
	[SetUp]
	public void OpenInteractive() => App.OpenScenario("Interactive");

	[Test]
	public void CalendarLayout_SwitchesBetweenWeekTwoWeeksAndMonthAndTheNewCellsCanBeTapped()
	{
		App.Tap("ActionLayout");
		Assert.That(App.WaitForText("VisibleDates", "2025-05-11..2025-05-17"), Is.EqualTo("2025-05-11..2025-05-17"));
		CalendarMatchesBaseline("Interactive_Week");

		App.TapDay(May(15));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-15"), Is.EqualTo("2025-05-15"));

		App.Tap("ActionLayout");
		Assert.That(App.WaitForText("VisibleDates", "2025-05-11..2025-05-24"), Is.EqualTo("2025-05-11..2025-05-24"));

		App.Tap("ActionLayout");
		Assert.That(App.WaitForText("VisibleDates", "2025-04-27..2025-06-07"), Is.EqualTo("2025-04-27..2025-06-07"));
		App.TapDay(May(28));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-28"), Is.EqualTo("2025-05-28"));
	}

	[Test]
	public void FirstDayOfWeek_RebuildsTheGridFromMonday()
	{
		App.Tap("ActionMonday");

		Assert.That(App.WaitForText("VisibleDates", "2025-04-28..2025-06-08"), Is.EqualTo("2025-04-28..2025-06-08"));
		Assert.That(App.Text("Title0"), Is.EqualTo("MON"));
		CalendarMatchesBaseline("Interactive_Monday");

		App.TapDay(May(20));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-20"), Is.EqualTo("2025-05-20"));
	}

	[Test]
	public void Events_AddedAndRemovedAtRuntime_UpdateTheIndicators()
	{
		App.Tap("ActionAddEvent");
		CalendarMatchesBaseline("Interactive_EventAdded");

		App.Tap("ActionRemoveEvents");
		CalendarMatchesBaseline("Default");
	}

	[Test]
	public void DisabledDates_ChangedAtRuntime_DisableTheDay()
	{
		App.Tap("ActionDisable");
		App.TapDay(May(16));

		Assert.That(App.Text("SelectedDate"), Is.EqualTo("none"));
		CalendarMatchesBaseline("Interactive_Disabled16");
	}

	[Test]
	public void Culture_ChangedAtRuntime_RewritesTheTexts()
	{
		App.Tap("ActionSelect21");
		App.Tap("ActionCulture");

		Assert.That(App.WaitForText("LayoutUnit", "Травень"), Is.EqualTo("Травень"));
		CalendarMatchesBaseline("Interactive_Ukrainian");

		App.Tap("ActionCulture");
		Assert.That(App.WaitForText("LayoutUnit", "May"), Is.EqualTo("May"));
		Assert.That(App.Text("SelectedText"), Is.EqualTo("21 May 2025"));
	}

	[Test]
	public void UseNativeDigits_ChangedAtRuntime_WritesTheCulturesDigits()
	{
		App.Tap("ActionSelect21");
		App.Tap("ActionNativeDigits");

		Assert.That(App.WaitForText("SelectedText", "٢١ مايو ٢٠٢٥"), Is.EqualTo("٢١ مايو ٢٠٢٥"));
		CalendarMatchesBaseline("Interactive_NativeDigits");

		App.Tap("ActionNativeDigits");
		Assert.That(App.WaitForText("SelectedText", "21 مايو 2025"), Is.EqualTo("21 مايو 2025"));
	}

	[Test]
	public void WeekViewUnit_ChangedAtRuntime_ShowsTheWeekNumber()
	{
		App.Tap("ActionWeekNumber");
		Assert.That(App.WaitForText("LayoutUnit", "20"), Is.EqualTo("20"));

		App.Tap("ActionWeekNumber");
		Assert.That(App.WaitForText("LayoutUnit", "May"), Is.EqualTo("May"));
	}

	[Test]
	public void DayViewTemplate_SetAndRemovedAtRuntime_SwapsTheCells()
	{
		App.Tap("ActionSelect21");
		App.Tap("ActionTemplate");
		CalendarMatchesBaseline("Interactive_Template");

		App.TapDay(May(9));
		Assert.That(App.WaitForText("SelectedDate", "2025-05-09"), Is.EqualTo("2025-05-09"));

		App.Tap("ActionTemplate");
		App.Tap("ActionClear");
		Assert.That(App.WaitForText("SelectedDate", "none"), Is.EqualTo("none"));
		App.Tap("ActionRemoveEvents");
		CalendarMatchesBaseline("Default");
	}

	[Test]
	public void ShownDate_SetFromCode_ShowsTheMonthWithoutRaisingMonthChanged()
	{
		App.Tap("ActionShowJuly");

		Assert.That(App.WaitForText("LayoutUnit", "July"), Is.EqualTo("July"));
		Assert.That(App.Text("ShownDate"), Is.EqualTo("2025-07-10"));
		Assert.That(App.Text("VisibleDates"), Is.EqualTo("2025-06-29..2025-08-09"));
		Assert.That(App.Text("MonthChanged"), Is.EqualTo("0"));
	}

	[Test]
	public void SelectionFromCode_SelectsAndClearSelectionClears()
	{
		App.Tap("ActionSelect21");
		Assert.That(App.WaitForText("SelectedDate", "2025-05-21"), Is.EqualTo("2025-05-21"));

		App.Tap("ActionClear");
		Assert.That(App.WaitForText("SelectedDate", "none"), Is.EqualTo("none"));
		Assert.That(App.Text("SelectedDates"), Is.EqualTo("-"));
	}

	[Test]
	public void CalendarSectionShown_ChangedFromCode_HidesAndShowsTheSection()
	{
		App.Tap("ActionToggleSection");
		Assert.That(App.WaitForText("SectionShown", "False"), Is.EqualTo("False"));

		App.Tap("ActionToggleSection");
		Assert.That(App.WaitForText("SectionShown", "True"), Is.EqualTo("True"));
		App.Tap("ActionRemoveEvents");
		CalendarMatchesBaseline("Default");
	}
}
