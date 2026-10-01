using System.Globalization;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Enums;
using static Plugin.Maui.Calendar.UITests.HostApp.SampleData;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.UITests.HostApp;

/// <summary>A page of the app: a calendar in a known state. The UI tests open it by <see cref="Name"/>.</summary>
public sealed record Scenario(string Name, Func<ScenarioPage> Create);

/// <summary>
/// Every scenario the UI tests open. Each one shows May 2025 (today is May 14) unless it says otherwise.
/// The tests in tests/Plugin.Maui.Calendar.UITests refer to these names.
/// </summary>
public static class ScenarioCatalog
{
	public static IReadOnlyList<Scenario> All { get; } =
	[
		// ── Looks ───────────────────────────────────────────────────────────
		Simple("Default", _ => { }),
		Simple("SelectedDay", calendar => calendar.SelectedDate = May(20)),
		Simple("SelectedToday", calendar => calendar.SelectedDate = TestEnvironment.Today),
		Simple("EventsBottomDot", calendar => calendar.Events = Events()),
		Simple("EventsTopDot", calendar => { calendar.Events = Events(); calendar.EventIndicatorType = EventIndicatorType.TopDot; }),
		Simple("EventsBackground", calendar => { calendar.Events = Events(); calendar.EventIndicatorType = EventIndicatorType.Background; calendar.SelectedDate = May(22); }),
		Simple("EventsBackgroundFull", calendar => { calendar.Events = Events(); calendar.EventIndicatorType = EventIndicatorType.BackgroundFull; }),
		Simple("DisabledDays", calendar =>
		{
			calendar.MinimumDate = May(6);
			calendar.MaximumDate = May(27);
			calendar.DisabledDates = [May(12), May(13)];
			calendar.DisabledDayColor = Colors.LightGray;
		}),
		Simple("OtherMonthDaysHidden", calendar => calendar.OtherMonthDayIsVisible = false),
		Simple("OtherMonthWeeksHidden", calendar => { calendar.OtherMonthDayIsVisible = false; calendar.OtherMonthWeekIsVisible = false; calendar.ShownDate = June(15); }),
		Simple("WeekLayout", calendar => calendar.CalendarLayout = WeekLayout.Week),
		Simple("TwoWeekLayout", calendar => calendar.CalendarLayout = WeekLayout.TwoWeek),
		Simple("WeekNumber", calendar =>
		{
			calendar.CalendarLayout = WeekLayout.Week;
			calendar.WeekViewUnit = WeekViewUnit.WeekNumber;
			calendar.FirstDayOfWeek = DayOfWeek.Monday;
		}),
		Simple("MondayFirst", calendar => calendar.FirstDayOfWeek = DayOfWeek.Monday),
		Simple("Weekend", calendar =>
		{
			calendar.WeekendDayColor = Colors.Crimson;
			calendar.WeekendTitleStyle = LabelStyle(Colors.Crimson, 18, FontAttributes.Bold);
			calendar.WeekendDayBackgroundColor = Color.FromArgb("#FFF0F0");
			calendar.WeekendDayBackgroundCornerRadius = 10;
		}),
		Simple("CustomStyles", calendar =>
		{
			calendar.SelectedDate = May(21);
			calendar.DayViewSize = 44;
			calendar.DayViewCornerRadius = 8;
			calendar.DayViewBorderMargin = new Thickness(2);
			calendar.DaysLabelStyle = LabelStyle(Colors.DarkSlateGray, 15, FontAttributes.Bold);
			calendar.DaysTitleLabelStyle = LabelStyle(Colors.SlateGray, 13);
			calendar.MonthLabelStyle = LabelStyle(Colors.DarkGreen, 22, FontAttributes.Bold);
			calendar.YearLabelStyle = LabelStyle(Colors.DarkGreen, 16);
			calendar.SelectedDateLabelStyle = LabelStyle(Colors.DarkGreen, 14);
			calendar.FooterArrowLabelStyle = LabelStyle(Colors.DarkGreen, 18);
			calendar.PreviousMonthArrowButtonStyle = ArrowStyle("<", Colors.DarkGreen);
			calendar.NextMonthArrowButtonStyle = ArrowStyle(">", Colors.DarkGreen);
			calendar.PreviousYearArrowButtonStyle = ArrowStyle("<", Colors.SeaGreen);
			calendar.NextYearArrowButtonStyle = ArrowStyle(">", Colors.SeaGreen);
			calendar.SelectedDayBackgroundColor = Colors.DarkGreen;
			calendar.SelectedDayTextColor = Colors.Yellow;
			calendar.DeselectedDayTextColor = Colors.DarkSlateGray;
			calendar.OtherMonthDayColor = Colors.Tan;
			calendar.TodayOutlineColor = Colors.Orange;
			calendar.TodayFillColor = Color.FromArgb("#FFF4E0");
			calendar.TodayTextColor = Colors.DarkOrange;
			calendar.SelectedDateTextFormat = "dddd, d MMMM";
		}),
		Simple("DayTitles", calendar =>
		{
			calendar.DaysTitleMaximumLength = DaysTitleMaxLength.TwoChars;
			calendar.DaysTitleLabelFirstUpperRestLower = true;
		}),
		Simple("Ukrainian", calendar =>
		{
			calendar.Culture = new CultureInfo("uk-UA");
			calendar.FirstDayOfWeek = DayOfWeek.Monday;
			calendar.UseAbbreviatedDayNames = true;
			calendar.SelectedDate = May(20);
		}),
		Simple("ArabicNativeDigits", calendar =>
		{
			calendar.Culture = new CultureInfo("ar-EG");
			calendar.FirstDayOfWeek = DayOfWeek.Saturday;
			calendar.UseNativeDigits = true;
			calendar.SelectedDate = May(20);
			calendar.FlowDirection = FlowDirection.RightToLeft;
		}),
		Simple("Persian", calendar =>
		{
			calendar.Culture = new CultureInfo("fa-IR");
			calendar.FirstDayOfWeek = DayOfWeek.Saturday;
			calendar.SelectedDate = May(20);
		}),
		Simple("RangeSelected", () => new RangeSelectionCalendar(), calendar =>
		{
			var range = (RangeSelectionCalendar)calendar;
			range.SelectedDatesRangeBackgroundColor = Color.FromArgb("#BBDEFB");
			range.SelectedStartDate = May(12);
			range.SelectedEndDate = May(16);
		}),
		Simple("MultiSelected", () => new MultiSelectionCalendar(), calendar => calendar.SelectedDates = [May(6), May(9), May(21)]),
		Simple("WeekSelected", () => new WeekSelectionCalendar(), calendar => calendar.SelectedDate = TestEnvironment.Today),
		Simple("DayTemplate", calendar =>
		{
			calendar.Events = Events();
			calendar.SelectedDate = May(21);
			calendar.DayViewTemplate = DayTemplate(Colors.SteelBlue);
		}),
		Simple("DayTemplateSelector", calendar =>
		{
			calendar.Events = Events();
			calendar.SelectedDate = May(21);
			calendar.DayViewTemplate = WeekendSelector();
		}),
		Simple("DayViewHeight", calendar =>
		{
			calendar.Events = Events();
			calendar.SelectedDate = May(20);
			calendar.DayViewHeight = 64;
			calendar.DayViewTemplate = EventNamesTemplate();
		}),
		Simple("HeaderFooterHidden", calendar => { calendar.HeaderSectionVisible = false; calendar.FooterSectionVisible = false; }),
		Simple("CustomHeaderFooter", calendar =>
		{
			calendar.SelectedDate = May(20);
			calendar.HeaderSectionTemplate = CustomHeader();
			calendar.FooterSectionTemplate = CustomFooter();
		}),
		Simple("NoYearPicker", calendar => { calendar.ShowYearPicker = false; calendar.FooterArrowVisible = false; calendar.SelectedDate = May(20); }),
		Scenario("EventsList", calendar =>
		{
			calendar.Events = Events();
			calendar.EventsScrollViewVisible = true;
			calendar.EventTemplate = EventTemplate();
			calendar.EmptyTemplate = EmptyEventsTemplate();
			calendar.SelectedDate = TestEnvironment.Today;
		}, calendarHeight: 560),
		Simple("SectionHidden", calendar => calendar.CalendarSectionShown = false),

		// ── Behavior ────────────────────────────────────────────────────────
		Scenario("Interactive", calendar => calendar.Events = Events(), actions: calendar =>
		[
			new("ActionLayout", "Layout", () => calendar.CalendarLayout = calendar.CalendarLayout switch
			{
				WeekLayout.Month => WeekLayout.Week,
				WeekLayout.Week => WeekLayout.TwoWeek,
				_ => WeekLayout.Month,
			}),
			new("ActionMonday", "Monday", () => calendar.FirstDayOfWeek = calendar.FirstDayOfWeek == DayOfWeek.Monday ? DayOfWeek.Sunday : DayOfWeek.Monday),
			new("ActionAddEvent", "Add event", () => calendar.Events.Add(May(15), new List<CalendarEvent> { new("Added") })),
			new("ActionRemoveEvents", "Remove events", () => calendar.Events.Clear()),
			new("ActionDisable", "Disable 16", () => calendar.DisabledDates = [May(16)]),
			new("ActionCulture", "Culture", () => calendar.Culture = calendar.Culture.Name == "uk-UA" ? TestEnvironment.Culture : new CultureInfo("uk-UA")),
			new("ActionNativeDigits", "Native digits", () =>
			{
				calendar.Culture = new CultureInfo("ar-EG");
				calendar.UseNativeDigits = !calendar.UseNativeDigits;
			}),
			new("ActionWeekNumber", "Week number", () => calendar.WeekViewUnit = calendar.WeekViewUnit == WeekViewUnit.MonthName ? WeekViewUnit.WeekNumber : WeekViewUnit.MonthName),
			new("ActionTemplate", "Template", () => calendar.DayViewTemplate = calendar.DayViewTemplate is null ? DayTemplate(Colors.SteelBlue) : null),
			new("ActionSelect21", "Select 21", () => calendar.SelectedDate = May(21)),
			new("ActionClear", "Clear", calendar.ClearSelection),
			new("ActionShowJuly", "July", () => calendar.ShownDate = new DateTime(2025, 7, 10)),
			new("ActionToggleSection", "Section", () => calendar.CalendarSectionShown = !calendar.CalendarSectionShown),
		]),
		Simple("Limits", calendar => { calendar.MinimumDate = May(6); calendar.MaximumDate = June(20); }),
		Simple("AutoChangeMonth", calendar => calendar.AutoChangeMonthOnDayTap = true),
		Simple("NoDeselect", calendar => { calendar.AllowDeselecting = false; calendar.SelectedDate = May(20); }),
		Simple("SwipeDisabled", calendar => calendar.SwipeDetectionDisabled = true),
		Simple("SwipeToChangeMonthOff", calendar => { calendar.SwipeToChangeMonthEnabled = false; calendar.SwipeUpToHideEnabled = false; }),
		Simple("Multi", () => new MultiSelectionCalendar(), _ => { }),
		Simple("Range", () => new RangeSelectionCalendar(), calendar => calendar.DisabledDates = [May(14)]),
		Simple("Week", () => new WeekSelectionCalendar(), calendar => calendar.FirstDayOfWeek = DayOfWeek.Monday),
	];

	public static Scenario? Find(string name) =>
		All.FirstOrDefault(scenario => string.Equals(scenario.Name, name, StringComparison.OrdinalIgnoreCase));

	static Scenario Simple(string name, Action<CalendarControl> configure) =>
		Scenario(name, configure);

	static Scenario Simple(string name, Func<CalendarControl> create, Action<CalendarControl> configure) =>
		Scenario(name, configure, create: create);

	static Scenario Scenario(
		string name,
		Action<CalendarControl> configure,
		Func<CalendarControl, IReadOnlyList<ScenarioAction>>? actions = null,
		Func<CalendarControl>? create = null,
		double? calendarHeight = null) =>
		new(name, () =>
		{
			var calendar = (create ?? (() => new CalendarControl()))();

			// The fixed environment first, so the scenario can change it.
			calendar.Culture = TestEnvironment.Culture;
			calendar.TimeProvider = TestEnvironment.TimeProvider;
			calendar.ShownDate = TestEnvironment.Today;

			configure(calendar);
			return new ScenarioPage(name, calendar, actions?.Invoke(calendar), calendarHeight);
		});
}
