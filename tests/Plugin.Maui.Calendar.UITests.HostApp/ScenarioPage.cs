using System.ComponentModel;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Models;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.UITests.HostApp;

/// <summary>A button of a scenario that changes its calendar at runtime.</summary>
public sealed record ScenarioAction(string AutomationId, string Text, Action Run);

/// <summary>
/// The page of a scenario: a <c>BackButton</c>, the calendar (<c>Calendar</c>), the calendar's state as
/// texts the UI tests read, and the scenario's action buttons.
/// </summary>
/// <remarks>
/// The calendar's own elements have no automation ids, so the page gives them some once they exist:
/// the arrows and labels of the default header and footer, the weekday titles (<c>Title0</c>..<c>Title6</c>),
/// the days grid (<c>DaysGrid</c>) and the day cells by their position (<c>Day0</c> is the first cell, the
/// day <c>VisibleDates</c> starts with). Cells are rebuilt when the layout changes, so new cells are tagged as
/// they are added: before they get a platform view, because Android ignores an automation id set later.
/// </remarks>
public sealed class ScenarioPage : ContentPage
{
	readonly CalendarControl calendar;
	readonly Dictionary<string, Label> statusValues = [];
	readonly List<string> monthChanges = [];
	readonly List<DateTime> dayTaps = [];
	int swipedLeft, swipedRight, swipedUp, swipedDown;

	public ScenarioPage(string name, CalendarControl calendar, IReadOnlyList<ScenarioAction>? actions = null, double? calendarHeight = null)
	{
		Title = name;
		this.calendar = calendar;

		calendar.AutomationId = "Calendar";
		calendar.VerticalOptions = LayoutOptions.Start;
		if (calendarHeight is { } height)
		{
			calendar.HeightRequest = height;
		}

		calendar.MonthChanged += (_, args) =>
		{
			monthChanges.Add($"{args.OldMonth:yyyy-MM-dd}>{args.NewMonth:yyyy-MM-dd}");
			UpdateStatus();
		};
		calendar.SwipedLeft += (_, _) => { swipedLeft++; UpdateStatus(); };
		calendar.SwipedRight += (_, _) => { swipedRight++; UpdateStatus(); };
		calendar.SwipedUp += (_, _) => { swipedUp++; UpdateStatus(); };
		calendar.SwipedDown += (_, _) => { swipedDown++; UpdateStatus(); };
		calendar.DayTappedCommand ??= new Command<DateTime>(date => { dayTaps.Add(date); UpdateStatus(); });
		calendar.PropertyChanged += OnCalendarPropertyChanged;
		foreach (var command in new[] { calendar.PrevLayoutUnitCommand, calendar.NextLayoutUnitCommand, calendar.PrevYearCommand, calendar.NextYearCommand })
		{
			command.CanExecuteChanged += (_, _) => UpdateStatus();
		}

		var back = new Button { AutomationId = "BackButton", Text = "Back", Padding = new Thickness(12, 4) };
		back.Clicked += (_, _) => App.Show(new GalleryPage());

		var actionButtons = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap };
		foreach (var action in actions ?? [])
		{
			var button = new Button { AutomationId = action.AutomationId, Text = action.Text, FontSize = 12, Margin = new Thickness(2), Padding = new Thickness(8, 2) };
			button.Clicked += (_, _) =>
			{
				action.Run();
				UpdateStatus();
			};
			actionButtons.Add(button);
		}

		var status = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 8 };
		foreach (var key in new[] { "SelectedDates", "SelectedDate", "ShownDate", "VisibleDates", "LayoutUnit", "SelectedText", "SectionShown", "MonthChanged", "DayTapped", "Swiped", "Arrows", "Range" })
		{
			var row = status.RowDefinitions.Count;
			status.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
			// The caption must not read like the id: iOS also finds an element by its text.
			status.Add(new Label { Text = key + ":", FontSize = 11, TextColor = Colors.Gray }, 0, row);
			var value = new Label { AutomationId = key, FontSize = 11, TextColor = Colors.Black };
			statusValues[key] = value;
			status.Add(value, 1, row);
		}

		var top = new HorizontalStackLayout
		{
			Spacing = 12,
			Children = { back, new Label { AutomationId = "ScenarioTitle", Text = name, VerticalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold } },
		};

		var details = new ScrollView { Content = new VerticalStackLayout { Spacing = 6, Children = { actionButtons, status } } };

		var layout = new Grid
		{
			RowDefinitions = [new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star)],
			Padding = new Thickness(8, 4),
			RowSpacing = 4,
			BackgroundColor = Colors.White,
		};
		layout.Add(top, 0, 0);
		layout.Add(calendar, 0, 1);
		layout.Add(details, 0, 2);

		BackgroundColor = Colors.White;
		Content = layout;

		TagCalendarElements();
		UpdateStatus();
	}

	void OnCalendarPropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateStatus();

	void TagCalendarElements()
	{
		var days = calendar.FindByName<Grid>("daysControl");
		Tag(days, "DaysGrid");

		foreach (var child in days.Children.OfType<View>())
		{
			TagDaysGridChild(child);
		}

		// A layout change replaces the titles and the cells; ChildAdded comes before their platform views.
		days.ChildAdded += (_, e) => TagDaysGridChild((View)e.Element);

		var sections = ((IVisualTreeElement)calendar).GetVisualTreeDescendants().OfType<DataTemplateView>().ToList();
		if (sections.Count == 2 && sections[0].Content is DefaultHeaderSection header)
		{
			var buttons = ((IVisualTreeElement)header).GetVisualTreeDescendants().OfType<Button>().ToList();
			Tag(buttons[0], "PrevMonthArrow");
			Tag(buttons[1], "NextMonthArrow");
			Tag(buttons[2], "PrevYearArrow");
			Tag(buttons[3], "NextYearArrow");

			var labels = ((IVisualTreeElement)header).GetVisualTreeDescendants().OfType<Label>().ToList();
			Tag(labels[0], "MonthLabel");
			Tag(labels[1], "YearLabel");
		}

		if (sections.Count == 2 && sections[1].Content is DefaultFooterSection footer)
		{
			var labels = ((IVisualTreeElement)footer).GetVisualTreeDescendants().OfType<Label>().ToList();
			Tag(labels[0], "SelectedDateLabel");
			Tag(labels[1], "FooterArrow");
		}
	}

	static void TagDaysGridChild(View child)
	{
		var row = Grid.GetRow(child);
		var column = Grid.GetColumn(child);

		if (child is Label && row == 0)
		{
			Tag(child, $"Title{column}");
		}
		else if (child is ContentView && row > 0)
		{
			// A day cell (a DayView); the weekend background boxes are Borders.
			Tag(child, $"Day{((row - 1) * 7) + column}");
		}
	}

	// An automation id can only be set once, so elements that already have one keep it.
	static void Tag(Element element, string automationId)
	{
		if (element is VisualElement { AutomationId: null } visualElement)
		{
			visualElement.AutomationId = automationId;
		}
	}

	void UpdateStatus()
	{
		Set("SelectedDates", string.Join(",", (calendar.SelectedDates ?? []).OrderBy(date => date).Select(Format)));
		Set("SelectedDate", calendar.SelectedDate is { } selected ? Format(selected) : "none");
		Set("ShownDate", Format(calendar.ShownDate));
		Set("VisibleDates", $"{Format(calendar.VisibleStartDate)}..{Format(calendar.VisibleEndDate)}");
		Set("LayoutUnit", calendar.LayoutUnitText ?? string.Empty);
		Set("SelectedText", calendar.SelectedDateText ?? string.Empty);
		Set("SectionShown", calendar.CalendarSectionShown.ToString());
		Set("MonthChanged", monthChanges.Count == 0 ? "0" : $"{monthChanges.Count} {monthChanges[^1]}");
		Set("DayTapped", dayTaps.Count == 0 ? "0" : $"{dayTaps.Count} {Format(dayTaps[^1])}");
		Set("Swiped", $"L{swipedLeft} R{swipedRight} U{swipedUp} D{swipedDown}");
		Set("Arrows", string.Join(",",
			calendar.PrevLayoutUnitCommand.CanExecute(null),
			calendar.NextLayoutUnitCommand.CanExecute(null),
			calendar.PrevYearCommand.CanExecute(null),
			calendar.NextYearCommand.CanExecute(null)));
		Set("Range", calendar is RangeSelectionCalendar range
			? $"{(range.SelectedStartDate is { } start ? Format(start) : "none")}..{(range.SelectedEndDate is { } end ? Format(end) : "none")}"
			: string.Empty);
	}

	// An empty label has no size, so UI automation would not find it: "-" stands for no value.
	void Set(string key, string value)
	{
		if (statusValues.TryGetValue(key, out var label))
		{
			label.Text = value.Length == 0 ? "-" : value;
		}
	}

	static string Format(DateTime date) => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}
