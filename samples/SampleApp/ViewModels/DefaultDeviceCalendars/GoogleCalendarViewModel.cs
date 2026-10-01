using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Plugin.Maui.Calendar.Models;
using SampleApp.Services;

namespace SampleApp.ViewModels;

/// <summary>
/// The Google Calendar app in four views: Schedule (the month drops down from the title, the events of
/// every day are listed under it), Day and Week (the events on the hours of the day, under the week strip
/// in the Week view) and Month (the events written in the days; a tapped day opens in the Day view).
/// The calendars of the drawer show or hide their events in all of them.
/// </summary>
public partial class GoogleCalendarViewModel : BasePageViewModel
{
	/// <summary>
	/// Height of an hour in the Day and Week views.
	/// </summary>
	public const double HourHeight = 52;

	/// <summary>
	/// Rows of the Month view: like Google Calendar, it always shows six weeks.
	/// </summary>
	public const int MonthWeekCount = 6;

	// Event colors of Google Calendar
	const string tomato = "#D50000";
	const string flamingo = "#E67C73";
	const string tangerine = "#F4511E";
	const string banana = "#F6BF26";
	const string sage = "#33B679";
	const string basil = "#0B8043";
	const string peacock = "#039BE5";
	const string blueberry = "#3F51B5";
	const string lavender = "#7986CB";
	const string grape = "#8E24AA";
	const string graphite = "#616161";

	static readonly Dictionary<(int Month, int Day), string> holidayNames = new()
	{
		[(1, 1)] = "New Year's Day",
		[(2, 14)] = "Valentine's Day",
		[(3, 8)] = "International Women's Day",
		[(3, 17)] = "St. Patrick's Day",
		[(4, 22)] = "Earth Day",
		[(5, 1)] = "Labour Day",
		[(6, 5)] = "World Environment Day",
		[(7, 30)] = "International Day of Friendship",
		[(8, 12)] = "International Youth Day",
		[(9, 21)] = "International Day of Peace",
		[(10, 4)] = "World Animal Day",
		[(10, 31)] = "Halloween",
		[(11, 13)] = "World Kindness Day",
		[(12, 24)] = "Christmas Eve",
		[(12, 25)] = "Christmas Day",
		[(12, 31)] = "New Year's Eve",
	};

	readonly GoogleCalendarSource personal = new("Personal", Color.FromArgb(peacock));
	readonly GoogleCalendarSource work = new("Work", Color.FromArgb(blueberry));
	readonly GoogleCalendarSource birthdays = new("Birthdays", Color.FromArgb(sage));
	readonly GoogleCalendarSource holidays = new("Holidays", Color.FromArgb(basil));

	readonly CultureInfo culture;
	readonly DayOfWeek firstDayOfWeek;
	readonly DateTime rangeStart;
	readonly DateTime rangeEnd;
	readonly List<GoogleEvent> events;

	// The day at the top of the schedule, kept to come back to it when the whole list is rebuilt
	DateTime topDate = DateTime.Today;

	// First day of the week in the Week view
	DateTime weekStart;

	// Lines that fit in a day of the Month view, set by the page from the height of the days
	int monthChipCapacity = 3;

	public GoogleCalendarViewModel(ICalendarSettingsService settings)
	{
		culture = settings.Culture;
		firstDayOfWeek = settings.FirstDayOfWeek;

		// The schedule covers two months back and four months ahead
		var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
		rangeStart = thisMonth.AddMonths(-2);
		rangeEnd = thisMonth.AddMonths(5).AddDays(-1);

		Calendars = [personal, work, birthdays, holidays];
		foreach (var calendar in Calendars)
		{
			calendar.PropertyChanged += OnCalendarPropertyChanged;
		}

		Views =
		[
			new(GoogleCalendarView.Schedule, "Schedule", "\uf03a"),
			new(GoogleCalendarView.Day, "Day", "\uf783"),
			new(GoogleCalendarView.Week, "Week", "\uf784"),
			new(GoogleCalendarView.Month, "Month", "\uf073") { IsSelected = true },
		];

		// "1 AM" ... "11 PM" on the hour lines of the Day and Week views; the first line has no label, as in Google Calendar
		var hourFormat = culture.DateTimeFormat.ShortTimePattern.Contains('t') ? "h tt" : culture.DateTimeFormat.ShortTimePattern;
		HourLabels = [.. Enumerable.Range(0, 24).Select(h => h == 0 ? string.Empty : DateTime.Today.AddHours(h).ToString(hourFormat, culture))];

		events = CreateSampleEvents();
		Events = CreateEventCollection();
		MonthEvents = CreateMonthEventCollection();
		ScheduleRows = new(CreateScheduleRows());
		weekStart = StartOfWeek(ShownDate);
		UpdateWeek();
		ShownDay = CreateDayColumn(ShownDate.Date, 0);
		MonthWeekdays = CreateMonthWeekdays(ShownDate);
		MonthStripItems = CreateMonthStripItems();
		SelectMonthStripItem(ShownDate);
	}

	/// <summary>
	/// Asks the page to scroll the schedule to a row; the flag tells whether to animate.
	/// </summary>
	public event Action<ScheduleRow, bool> ScrollRequested;

	public IReadOnlyList<GoogleCalendarSource> Calendars { get; }

	/// <summary>
	/// The views listed in the drawer.
	/// </summary>
	public IReadOnlyList<GoogleCalendarViewOption> Views { get; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsScheduleView), nameof(IsDayView), nameof(IsWeekView), nameof(IsMonthView))]
	public partial GoogleCalendarView CurrentView { get; private set; } = GoogleCalendarView.Month;

	public bool IsScheduleView => CurrentView == GoogleCalendarView.Schedule;

	public bool IsDayView => CurrentView == GoogleCalendarView.Day;

	public bool IsWeekView => CurrentView == GoogleCalendarView.Week;

	public bool IsMonthView => CurrentView == GoogleCalendarView.Month;

	/// <summary>
	/// Events of the visible calendars by day; the drop-down month puts a dot under these days.
	/// </summary>
	[ObservableProperty]
	public partial EventCollection Events { get; private set; }

	/// <summary>
	/// The same events as <see cref="Events"/>, as the lines that the days of the Month view show.
	/// </summary>
	[ObservableProperty]
	public partial EventCollection MonthEvents { get; private set; }

	[ObservableProperty]
	public partial ObservableCollection<ScheduleRow> ScheduleRows { get; private set; }

	/// <summary>
	/// The day of the Day view: the day of <see cref="ShownDate"/>.
	/// </summary>
	[ObservableProperty]
	public partial DayColumn ShownDay { get; private set; }

	/// <summary>
	/// The seven days of the Week view.
	/// </summary>
	[ObservableProperty]
	public partial IReadOnlyList<DayColumn> WeekDays { get; private set; }

	[ObservableProperty]
	public partial bool WeekHasAllDayEvents { get; private set; }

	public IReadOnlyList<string> HourLabels { get; }

	/// <summary>
	/// Month of the title and of the drop-down and Month view calendars, week of the Week view,
	/// day of the Day view. Follows the schedule while it scrolls.
	/// </summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Title))]
	public partial DateTime ShownDate { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; } = DateTime.Today;

	/// <summary>
	/// The event shown full screen, null when none is.
	/// </summary>
	[ObservableProperty]
	public partial GoogleEvent OpenedEvent { get; set; }

	public string Title => MonthTitle(ShownDate);

	/// <summary>
	/// The months to pick in the Month view, two years back and three ahead.
	/// </summary>
	public IReadOnlyList<MonthStripItem> MonthStripItems { get; }

	/// <summary>
	/// The weekdays over the Month view, from the first day of the week.
	/// </summary>
	[ObservableProperty]
	public partial IReadOnlyList<WeekdayTitle> MonthWeekdays { get; private set; }

	public string TodayText => DateTime.Today.Day.ToString(culture);

	IEnumerable<GoogleEvent> VisibleEvents => events.Where(e => e.Calendar.IsVisible);

	/// <summary>
	/// Called by the page while the schedule scrolls: the title and the drop-down month follow its top row.
	/// </summary>
	public void OnScheduleScrolled(int firstVisibleIndex)
	{
		if (firstVisibleIndex < 0 || firstVisibleIndex >= ScheduleRows.Count)
		{
			return;
		}

		topDate = ScheduleRows[firstVisibleIndex].Date;

		if (topDate.Year != ShownDate.Year || topDate.Month != ShownDate.Month)
		{
			ShownDate = topDate;
		}
	}

	/// <summary>
	/// Called by the page when the days of the Month view change height: fits their lines to it.
	/// </summary>
	public void SetMonthChipCapacity(int capacity)
	{
		if (capacity != monthChipCapacity)
		{
			monthChipCapacity = capacity;
			MonthEvents = CreateMonthEventCollection();
		}
	}

	partial void OnShownDateChanged(DateTime value)
	{
		if (StartOfWeek(value) != weekStart)
		{
			weekStart = StartOfWeek(value);
			UpdateWeek();
		}

		if (value.Date != ShownDay.Date)
		{
			ShownDay = CreateDayColumn(value.Date, 0);
		}

		MonthWeekdays = CreateMonthWeekdays(value);
		SelectMonthStripItem(value);
	}

	// A day tapped in the drop-down month: the schedule scrolls to it, the Day and Week views show it
	partial void OnSelectedDateChanged(DateTime? value)
	{
		if (value is DateTime date)
		{
			ShownDate = date;

			if (IsScheduleView)
			{
				ScrollTo(date, animate: true);
			}
		}
	}

	[RelayCommand]
	void Today() => GoTo(DateTime.Today);

	[RelayCommand]
	void SelectView(GoogleCalendarViewOption option) => ShowView(option.View);

	// A month picked in the month strip of the Month view; its year labels do nothing
	[RelayCommand]
	void ShowMonth(MonthStripItem item)
	{
		if (item.IsMonth)
		{
			ShownDate = item.Date;
		}
	}

	[RelayCommand]
	void PreviousDay() => GoTo(ShownDate.Date.AddDays(-1));

	[RelayCommand]
	void NextDay() => GoTo(ShownDate.Date.AddDays(1));

	[RelayCommand]
	void PreviousWeek() => ShownDate = ShownDate.AddDays(-7);

	[RelayCommand]
	void NextWeek() => ShownDate = ShownDate.AddDays(7);

	// A day tapped in the Month view or in the week strip opens in the Day view, as in Google Calendar
	[RelayCommand]
	void ShowDay(DateTime day)
	{
		ShowView(GoogleCalendarView.Day);
		GoTo(day);
	}

	[RelayCommand]
	void OpenEvent(GoogleEvent googleEvent) => OpenedEvent = googleEvent;

	[RelayCommand]
	void CloseEvent() => OpenedEvent = null;

	[RelayCommand]
	void DeleteEvent()
	{
		if (OpenedEvent is not { } deleted)
		{
			return;
		}

		events.Remove(deleted);
		OpenedEvent = null;
		UpdateDay(deleted.Start.Date);
	}

	// Cancelled by the page when it disappears
	[RelayCommand]
	async Task AddEvent(CancellationToken token)
	{
		var title = await Shell.Current.CurrentPage.DisplayPromptAsync("New event", null, "Save", "Cancel", "Add title");

		if (token.IsCancellationRequested || string.IsNullOrWhiteSpace(title))
		{
			return;
		}

		// On the day on screen in the Day view, else on the day selected in the drop-down month.
		// Like Google Calendar: at the next full hour today, at 9:00 on another day.
		var day = IsDayView ? ShownDate.Date : SelectedDate ?? DateTime.Today;
		var start = day == DateTime.Today
			? day.AddHours(Math.Min(DateTime.Now.Hour + 1, 23))
			: day.AddHours(9);

		personal.IsVisible = true;
		events.Add(new GoogleEvent
		{
			Title = title.Trim(),
			Start = start,
			End = start.AddHours(1),
			Calendar = personal,
		}.Localize(culture));

		UpdateDay(day);
		GoTo(day);
	}

	// Cancelled by the page when it disappears
	[RelayCommand]
	async Task Search(CancellationToken token)
	{
		var query = await Shell.Current.CurrentPage.DisplayPromptAsync("Search", null, "Search", "Cancel", "Search events");

		if (token.IsCancellationRequested || string.IsNullOrWhiteSpace(query))
		{
			return;
		}

		query = query.Trim();

		// The nearest upcoming match, else the latest past one
		var matches = VisibleEvents
			.Where(e => culture.CompareInfo.IndexOf(e.Title, query, CompareOptions.IgnoreCase) >= 0)
			.OrderBy(e => e.Start)
			.ToList();
		var match = matches.FirstOrDefault(e => e.Start.Date >= DateTime.Today) ?? matches.LastOrDefault();

		if (match is null)
		{
			await Shell.Current.DisplayAlertAsync("Search", $"No results for \"{query}\"", "OK");
			return;
		}

		if (token.IsCancellationRequested)
		{
			return;
		}

		GoTo(match.Start.Date);
		OpenedEvent = match;
	}

	void ShowView(GoogleCalendarView view)
	{
		foreach (var option in Views)
		{
			option.IsSelected = option.View == view;
		}

		CurrentView = view;

		if (IsScheduleView)
		{
			ScrollTo(ShownDate, animate: false);
		}
	}

	void GoTo(DateTime date)
	{
		ShownDate = date;

		// Selecting the day scrolls the schedule to it; an already selected day does not change, so scroll here
		if (SelectedDate != date)
		{
			SelectedDate = date;
		}
		else if (IsScheduleView)
		{
			ScrollTo(date, animate: true);
		}
	}

	/// <summary>
	/// Asks the page to scroll the schedule to a day: to its first row (month banner, week label or events),
	/// or to the label of its week when the day has no events.
	/// </summary>
	public void ScrollTo(DateTime date, bool animate)
	{
		var row = ScheduleRows.FirstOrDefault(r => r.Date == date.Date)
			?? ScheduleRows.LastOrDefault(r => r.Date < date.Date && r is ScheduleWeekRow)
			?? ScheduleRows.FirstOrDefault();

		if (row is not null)
		{
			ScrollRequested?.Invoke(row, animate);
		}
	}

	void OnCalendarPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(GoogleCalendarSource.IsVisible))
		{
			return;
		}

		// A calendar was shown or hidden in the drawer: its events leave or join every day
		Events = CreateEventCollection();
		MonthEvents = CreateMonthEventCollection();
		ScheduleRows = new(CreateScheduleRows());
		UpdateWeek();
		ShownDay = CreateDayColumn(ShownDay.Date, 0);

		// A new list starts at the top: go back to where the schedule was
		ScrollTo(topDate, animate: false);
	}

	/// <summary>
	/// Refreshes one day after an event was added or deleted: its dot in the drop-down month,
	/// its row in the schedule, its lines in the Month view and its hours in the Day and Week views.
	/// </summary>
	void UpdateDay(DateTime day)
	{
		var dayEvents = EventsOn(day);

		// EventCollection notices a day being set or removed, not a change inside the list of a day,
		// so the day gets a new list
		if (dayEvents.Count > 0)
		{
			Events[day] = dayEvents;
			MonthEvents[day] = MonthChipsOn(dayEvents);
		}
		else
		{
			Events.Remove(day);
			MonthEvents.Remove(day);
		}

		if (day >= weekStart && day < weekStart.AddDays(7))
		{
			UpdateWeek();
		}

		if (ShownDay.Date == day)
		{
			ShownDay = CreateDayColumn(day, 0);
		}

		var oldRow = ScheduleRows.OfType<ScheduleDayRow>().FirstOrDefault(r => r.Date == day);
		var newRow = CreateDayRow(day, dayEvents);

		if (oldRow is not null)
		{
			var index = ScheduleRows.IndexOf(oldRow);

			if (newRow is null)
			{
				ScheduleRows.RemoveAt(index);
			}
			else
			{
				ScheduleRows[index] = newRow;
			}
		}
		else if (newRow is not null)
		{
			// After the month banner and the week label of that day
			ScheduleRows.Insert(ScheduleRows.TakeWhile(r => r.Date <= day).Count(), newRow);
		}
	}

	List<GoogleEvent> EventsOn(DateTime day) =>
	[
		.. VisibleEvents
			.Where(e => e.Start.Date == day)
			.OrderByDescending(e => e.IsAllDay)
			.ThenBy(e => e.Start),
	];

	EventCollection CreateEventCollection()
	{
		EventCollection collection = new();

		foreach (var day in VisibleEvents.Select(e => e.Start.Date).Distinct())
		{
			collection[day] = EventsOn(day);
		}

		return collection;
	}

	EventCollection CreateMonthEventCollection()
	{
		EventCollection collection = new();

		foreach (var day in VisibleEvents.Select(e => e.Start.Date).Distinct())
		{
			collection[day] = MonthChipsOn(EventsOn(day));
		}

		return collection;
	}

	// The events that fit in the day, and "+2" for the others
	List<MonthChip> MonthChipsOn(List<GoogleEvent> dayEvents)
	{
		if (dayEvents.Count <= monthChipCapacity)
		{
			return [.. dayEvents.Select(MonthChip.For)];
		}

		var shown = Math.Max(monthChipCapacity - 1, 0);
		return [.. dayEvents.Take(shown).Select(MonthChip.For), MonthChip.More(dayEvents.Count - shown)];
	}

	void UpdateWeek()
	{
		WeekDays = [.. Enumerable.Range(0, 7).Select(column => CreateDayColumn(weekStart.AddDays(column), column))];
		WeekHasAllDayEvents = WeekDays.Any(d => d.HasAllDayEvents);
	}

	DayColumn CreateDayColumn(DateTime day, int column)
	{
		var dayEvents = EventsOn(day);

		return new DayColumn(
			column,
			day,
			day.ToString("ddd", culture),
			day.Day.ToString(culture),
			[.. dayEvents.Where(e => e.IsAllDay)],
			CreateEventBlocks([.. dayEvents.Where(e => !e.IsAllDay)]),
			HourHeight);
	}

	// Events that overlap share the width of the day, each in its own lane, as in Google Calendar
	static List<EventBlock> CreateEventBlocks(List<GoogleEvent> timedEvents)
	{
		List<EventBlock> blocks = [];
		List<(GoogleEvent Event, int Lane)> group = [];
		List<DateTime> laneEnds = [];
		var groupEnd = DateTime.MinValue;

		void CloseGroup()
		{
			blocks.AddRange(group.Select(g => new EventBlock(g.Event, g.Lane, laneEnds.Count, HourHeight)));
			group.Clear();
			laneEnds.Clear();
		}

		foreach (var timedEvent in timedEvents)
		{
			if (group.Count > 0 && timedEvent.Start >= groupEnd)
			{
				CloseGroup();
			}

			var lane = laneEnds.FindIndex(end => end <= timedEvent.Start);

			if (lane < 0)
			{
				lane = laneEnds.Count;
				laneEnds.Add(timedEvent.End);
			}
			else
			{
				laneEnds[lane] = timedEvent.End;
			}

			group.Add((timedEvent, lane));

			if (group.Count == 1 || timedEvent.End > groupEnd)
			{
				groupEnd = timedEvent.End;
			}
		}

		CloseGroup();
		return blocks;
	}

	// "Sep", "Oct", "Nov", "Dec", "2027", "Jan" ..., as in Google Calendar
	List<MonthStripItem> CreateMonthStripItems()
	{
		List<MonthStripItem> items = [];

		for (var month = new DateTime(DateTime.Today.Year - 2, 1, 1); month.Year <= DateTime.Today.Year + 3; month = month.AddMonths(1))
		{
			if (month.Month == 1)
			{
				items.Add(MonthStripItem.ForYear(month, month.Year.ToString(culture)));
			}

			var name = culture.DateTimeFormat.GetAbbreviatedMonthName(month.Month);
			items.Add(MonthStripItem.ForMonth(month, culture.TextInfo.ToUpper(name[0]) + name[1..]));
		}

		return items;
	}

	void SelectMonthStripItem(DateTime shownDate)
	{
		foreach (var item in MonthStripItems)
		{
			item.IsSelected = item.IsMonth && item.Date.Year == shownDate.Year && item.Date.Month == shownDate.Month;
		}
	}

	// "Mon", "Tue" ...; today's weekday is highlighted while today is one of the six weeks on screen
	List<WeekdayTitle> CreateMonthWeekdays(DateTime shownDate)
	{
		var firstShown = StartOfWeek(new DateTime(shownDate.Year, shownDate.Month, 1));
		var todayShown = DateTime.Today >= firstShown && DateTime.Today < firstShown.AddDays(7 * MonthWeekCount);

		return
		[
			.. Enumerable.Range(0, 7).Select(column =>
			{
				var dayOfWeek = (DayOfWeek)(((int)firstDayOfWeek + column) % 7);
				var name = culture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek);
				name = culture.TextInfo.ToUpper(name[0]) + name[1..];

				return new WeekdayTitle(column, name, todayShown && dayOfWeek == DateTime.Today.DayOfWeek);
			}),
		];
	}

	DateTime StartOfWeek(DateTime date) =>
		date.Date.AddDays(-((7 + (date.DayOfWeek - firstDayOfWeek)) % 7));

	List<ScheduleRow> CreateScheduleRows()
	{
		List<ScheduleRow> rows = [];

		for (var day = rangeStart; day <= rangeEnd; day = day.AddDays(1))
		{
			if (day.Day == 1)
			{
				rows.Add(new ScheduleMonthRow(day, MonthTitle(day)));
			}

			// Weeks are cut at the end of a month, so every month starts with its own week label
			if (day.Day == 1 || day.DayOfWeek == firstDayOfWeek)
			{
				rows.Add(new ScheduleWeekRow(day, WeekText(day)));
			}

			if (CreateDayRow(day, EventsOn(day)) is { } dayRow)
			{
				rows.Add(dayRow);
			}
		}

		return rows;
	}

	ScheduleDayRow CreateDayRow(DateTime day, List<GoogleEvent> dayEvents)
	{
		if (dayEvents.Count == 0)
		{
			return null;
		}

		List<object> items = [.. dayEvents];

		if (day == DateTime.Today)
		{
			// The current time line goes before the first event that has not started yet
			var next = dayEvents.FindIndex(e => !e.IsAllDay && e.Start > DateTime.Now);
			items.Insert(next < 0 ? items.Count : next, new ScheduleNowLine());
		}

		return new ScheduleDayRow(day, day.ToString("ddd", culture), day.Day.ToString(culture), items);
	}

	string MonthTitle(DateTime date)
	{
		var name = culture.DateTimeFormat.GetMonthName(date.Month);
		name = culture.TextInfo.ToUpper(name[0]) + name[1..];

		// Like Google Calendar: the year only when it is not the current one
		return date.Year == DateTime.Today.Year ? name : $"{name} {date.Year}";
	}

	// "Oct 5 – 11", from a week's first day to its last one or to the end of the month
	string WeekText(DateTime start)
	{
		var weekEnd = StartOfWeek(start).AddDays(6);
		var monthEnd = new DateTime(start.Year, start.Month, 1).AddMonths(1).AddDays(-1);
		var end = weekEnd < monthEnd ? weekEnd : monthEnd;

		var pattern = culture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM");

		if (start == end)
		{
			return start.ToString(pattern, culture);
		}

		// The culture decides whether the month goes first ("Oct 5 – 11") or last ("5 – 11 Oct")
		return pattern.IndexOf('M') < pattern.IndexOf('d')
			? $"{start.ToString(pattern, culture)} – {end.Day.ToString(culture)}"
			: $"{start.Day.ToString(culture)} – {end.ToString(pattern, culture)}";
	}

	List<GoogleEvent> CreateSampleEvents()
	{
		var today = DateTime.Today;
		List<GoogleEvent> list = [];

		void Timed(string title, DateTime start, double hours, GoogleCalendarSource calendar, string location = null, string color = null, string description = null) =>
			list.Add(new GoogleEvent
			{
				Title = title,
				Start = start,
				End = start.AddHours(hours),
				Calendar = calendar,
				Location = location,
				Description = description,
				EventColor = color is null ? null : Color.FromArgb(color),
			}.Localize(culture));

		void AllDay(string title, DateTime day, GoogleCalendarSource calendar, string color = null, string description = null) =>
			list.Add(new GoogleEvent
			{
				Title = title,
				Start = day,
				End = day.AddDays(1),
				IsAllDay = true,
				Calendar = calendar,
				Description = description,
				EventColor = color is null ? null : Color.FromArgb(color),
			}.Localize(culture));

		// Weekly events and holidays over the whole schedule
		for (var day = rangeStart; day <= rangeEnd; day = day.AddDays(1))
		{
			switch (day.DayOfWeek)
			{
				case DayOfWeek.Monday:
					Timed("Weekly sync", day.AddHours(10), 0.5, work, "Meeting room 2");
					break;
				case DayOfWeek.Wednesday:
					Timed("Yoga", day.AddHours(18.5), 1, personal, "Flow Studio", tangerine);
					break;
				case DayOfWeek.Friday:
					Timed("Team lunch", day.AddHours(13), 1, work, "Pasta Bar");
					break;
			}

			if (holidayNames.TryGetValue((day.Month, day.Day), out var holiday))
			{
				AllDay(holiday, day, holidays);
			}
		}

		// A fixed date: October 23 of this year
		AllDay(".NET MAUI Day", new DateTime(today.Year, 10, 23), work, grape, "A day of talks about .NET MAUI.");

		// Past days
		AllDay("Max's birthday", today.AddDays(-12), birthdays);
		Timed("Movie night", today.AddDays(-15).AddHours(20), 2, personal, "Cinema City", grape);
		Timed("Interview", today.AddDays(-9).AddHours(11), 1, work, "Video call", description: "Mobile developer candidate.");
		Timed("Car service", today.AddDays(-7).AddHours(8), 1, personal, "AutoFix", graphite);
		Timed("Retro", today.AddDays(-4).AddHours(15), 1, work, "Meeting room 2");
		Timed("Book club", today.AddDays(-3).AddHours(19), 1.5, personal, "Library café", sage);
		Timed("Haircut", today.AddDays(-1).AddHours(10), 0.5, personal, "Barber & Co");

		// Today and later
		Timed("Design review", today.AddHours(11), 1, work, "Meeting room 4", description: "Walk through the new calendar screens with the team.");
		Timed("Coffee with Anna", today.AddHours(15.5), 0.5, personal, "Blue Bottle Coffee");
		Timed("Dentist", today.AddDays(1).AddHours(9), 0.5, personal, "Smile Dental Clinic", flamingo);
		AllDay("Anna's birthday", today.AddDays(2), birthdays);
		Timed("Sprint planning", today.AddDays(3).AddHours(14), 1, work, "Meeting room 2", description: "Pick the stories of the next sprint.");
		Timed("Jazz concert", today.AddDays(6).AddHours(19), 2.5, personal, "City Hall", grape);
		Timed("Flight to Lisbon", today.AddDays(8).AddHours(8.25), 3.5, personal, "Terminal D", lavender, "Booking reference XK7Q2P");
		Timed("Release 3.2", today.AddDays(10).AddHours(12), 1, work, color: tomato, description: "Publish the NuGet package and the release notes.");
		Timed("Quarterly review", today.AddDays(14).AddHours(10), 2, work, "Main hall");
		AllDay("Payday", today.AddDays(15), personal, banana);
		Timed("Parent-teacher meeting", today.AddDays(18).AddHours(17), 1, personal, "Oak Street School");
		AllDay("Dad's birthday", today.AddDays(19), birthdays);
		AllDay("Weekend in the mountains", today.AddDays(23), personal, sage);
		Timed("Doctor appointment", today.AddDays(26).AddHours(12), 0.5, personal, "City Clinic, room 12", flamingo);
		Timed("Anniversary dinner", today.AddDays(31).AddHours(19.5), 2, personal, "La Piazza", grape);
		Timed(".NET MAUI meetup", today.AddDays(37).AddHours(18), 2, work, "Tech Hub");
		AllDay("Car insurance renewal", today.AddDays(45), personal, graphite);
		AllDay("Team offsite", today.AddDays(52), work);
		Timed("Dentist check-up", today.AddDays(60).AddHours(9), 0.5, personal, "Smile Dental Clinic", flamingo);

		return list;
	}
}