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
	double scrollY;

	/// <param name="inScrollView">
	/// Puts the calendar in a <c>CalendarScroll</c> ScrollView with content below it, so the page scrolls:
	/// how far it scrolled is shown in <c>ScrollY</c>.
	/// </param>
	public ScenarioPage(string name, CalendarControl calendar, IReadOnlyList<ScenarioAction>? actions = null, double? calendarHeight = null, bool inScrollView = false)
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
		foreach (var key in new[] { "SelectedDates", "SelectedDate", "ShownDate", "VisibleDates", "LayoutUnit", "SelectedText", "SectionShown", "MonthChanged", "DayTapped", "Swiped", "Arrows", "Range", "ScrollY", "DrawnTitles", "DrawnCells" })
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
		if (inScrollView)
		{
			var scroll = new ScrollView
			{
				AutomationId = "CalendarScroll",
				HeightRequest = 420,
				Content = new VerticalStackLayout { Children = { calendar, new BoxView { HeightRequest = 1200, Color = Colors.LightBlue } } },
			};
			scroll.Scrolled += (_, e) =>
			{
				scrollY = e.ScrollY;
				UpdateStatus();
			};
			layout.Add(scroll, 0, 1);
		}
		else
		{
			layout.Add(calendar, 0, 1);
		}
		layout.Add(details, 0, 2);

		BackgroundColor = Colors.White;
		Content = layout;

		TagCalendarElements();
		UpdateStatus();

		// The drawn day grid has no platform views to find: where its cells are is reported as text.
		Dispatcher.StartTimer(TimeSpan.FromMilliseconds(250), () =>
		{
			UpdateDrawnGridStatus();
			return Window is not null || Handler is null;
		});
	}

	/// <summary>
	/// With the drawn day grid (see DrawnMode), shows its weekday titles (<c>DrawnTitles</c>: the 7 texts) and
	/// where its days are (<c>DrawnCells</c>: the calendar's width, then the center of every cell, in
	/// device-independent units from the calendar's top left corner, in the order of <c>Day0</c>..<c>Day41</c>).
	/// The UI tests scale the positions by the size the calendar has on screen.
	/// </summary>
	void UpdateDrawnGridStatus()
	{
		// The grid drawn with Microsoft.Maui.Graphics is a view of its own, which knows where its cells are.
		if (calendar.Handler is not null && calendar.GraphicsDayGrid is { Width: > 0 } graphicsGrid)
		{
			var invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
			var gridOrigin = OffsetIn(graphicsGrid, calendar);
			var drawable = graphicsGrid.GridDrawable;
			var centers = Enumerable.Range(0, drawable.Days.Count).Select(index =>
			{
				var center = drawable.CellBounds(index, (float)graphicsGrid.Width).Center;
				return string.Format(invariantCulture, "{0:0.#},{1:0.#}", gridOrigin.X + center.X, gridOrigin.Y + center.Y);
			});

			Set("DrawnTitles", string.Join(",", drawable.ShownTitles));
			Set("DrawnCells", string.Format(invariantCulture, "{0:0.#}|", calendar.Width) + string.Join(";", centers));
			return;
		}

		if (calendar.Handler is null || calendar.DrawnDayGrid is not { } grid || grid.Superview is not VisualElement canvas)
		{
			Set("DrawnTitles", string.Empty);
			Set("DrawnCells", string.Empty);
			return;
		}

		var invariant = System.Globalization.CultureInfo.InvariantCulture;
		var scale = grid.RenderingScale;
		var origin = OffsetIn(canvas, calendar);
		var cells = grid.Cells.Select(cell => string.Format(invariant, "{0:0.#},{1:0.#}",
			origin.X + (cell.DrawingRect.MidX / scale),
			origin.Y + (cell.DrawingRect.MidY / scale)));

		Set("DrawnTitles", string.Join(",", grid.TitleLabels.Select(label => label.Text)));
		Set("DrawnCells", string.Format(invariant, "{0:0.#}|", calendar.Width) + string.Join(";", cells));
	}

	// The position of view's top left corner in ancestor.
	static Point OffsetIn(VisualElement view, Element ancestor)
	{
		double x = 0, y = 0;
		for (Element element = view; element is not null && element != ancestor; element = element.Parent)
		{
			if (element is VisualElement visual)
			{
				x += visual.X + visual.TranslationX;
				y += visual.Y + visual.TranslationY;
			}
		}

		return new Point(x, y);
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
		Set("ScrollY", scrollY.ToString("0", System.Globalization.CultureInfo.InvariantCulture));
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
