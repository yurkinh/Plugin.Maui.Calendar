using NUnit.Framework;
using Plugin.Maui.Calendar.UITests.Infrastructure;

namespace Plugin.Maui.Calendar.UITests.Tests;

/// <summary>
/// Compares a screenshot of the calendar in every look scenario of the host app with its baseline: the
/// default calendar, selections, event indicators, disabled and hidden days, the layouts, colors and styles,
/// cultures, day templates, header and footer options and the events list.
/// </summary>
public class AppearanceTests : UITest
{
	static readonly string[] scenarios =
	[
		"Default",
		"SelectedDay",
		"SelectedToday",
		"EventsBottomDot",
		"EventsTopDot",
		"EventsBackground",
		"EventsBackgroundFull",
		"DisabledDays",
		"OtherMonthDaysHidden",
		"OtherMonthWeeksHidden",
		"WeekLayout",
		"TwoWeekLayout",
		"WeekNumber",
		"MondayFirst",
		"Weekend",
		"CustomStyles",
		"DayTitles",
		"Ukrainian",
		"ArabicNativeDigits",
		"Persian",
		"RangeSelected",
		"MultiSelected",
		"WeekSelected",
		"DayTemplate",
		"DayTemplateSelector",
		"DayViewHeight",
		"HeaderFooterHidden",
		"CustomHeaderFooter",
		"NoYearPicker",
		"EventsList",
		"SectionHidden",
	];

	[TestCaseSource(nameof(scenarios))]
	public void Scenario_LooksLikeItsBaseline(string scenario)
	{
		App.OpenScenario(scenario);

		CalendarMatchesBaseline(scenario);
	}
}
