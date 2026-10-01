using System.Collections;
using System.Globalization;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	// The week shown in a row starts on FirstDayOfWeek, so the week is counted from that day too;
	// otherwise the number of the shown row would depend on which of its days ShownDate is.
	int GetWeekNumber(DateTime date)
	{
		return FormattingCulture.DateTimeFormat.Calendar.GetWeekOfYear(
			date,
			CalendarWeekRule.FirstFourDayWeek,
			FirstDayOfWeek
		);
	}

	void PrevUnit()
	{
		if (!CanExecutePrevUnit())
		{
			return;
		}

		var oldShownDate = ShownDate;
		ShownDate = CurrentViewLayoutEngine.GetPreviousUnit(ShownDate);
		RaiseMonthChanged(oldShownDate);
	}

	void NextUnit()
	{
		if (!CanExecuteNextUnit())
		{
			return;
		}

		var oldShownDate = ShownDate;
		ShownDate = CurrentViewLayoutEngine.GetNextUnit(ShownDate);
		RaiseMonthChanged(oldShownDate);
	}

	// Moving back is only blocked by MinimumDate and moving forward only by MaximumDate, so a
	// calendar showing a month outside the allowed dates can always be moved towards them.
	bool CanExecutePrevUnit()
	{
		var target = CurrentViewLayoutEngine.GetPreviousUnit(ShownDate);
		return target != ShownDate && GetShownUnit(target).End >= MinimumDate.Date;
	}

	bool CanExecuteNextUnit()
	{
		var target = CurrentViewLayoutEngine.GetNextUnit(ShownDate);
		return target != ShownDate && GetShownUnit(target).Start <= MaximumDate.Date;
	}

	/// <summary>
	/// The days of the unit the calendar shows for <paramref name="date"/>: its month in the month
	/// layout (the days of other months around it don't count), its week(s) in the week layouts.
	/// </summary>
	(DateTime Start, DateTime End) GetShownUnit(DateTime date)
	{
		if (CalendarLayout == WeekLayout.Month)
		{
			return (new DateTime(date.Year, date.Month, 1), new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)));
		}

		return (CurrentViewLayoutEngine.GetFirstDate(date).Date, CurrentViewLayoutEngine.GetLastDate(date).Date);
	}

	void NextYear(object obj)
	{
		if (!CanExecuteNextYear(obj))
		{
			return;
		}

		var oldShownDate = ShownDate;
		var target = ShownDate.AddYears(1);
		ShownDate = target > MaximumDate ? MaximumDate : target;
		RaiseMonthChanged(oldShownDate);
	}

	// MaximumDate.Year is at most DateTime.MaxValue.Year, so this also keeps AddYears in range.
	bool CanExecuteNextYear(object obj) => ShownDate.Year < MaximumDate.Year;

	void PrevYear(object obj)
	{
		if (!CanExecutePrevYear(obj))
		{
			return;
		}

		var oldShownDate = ShownDate;
		var target = ShownDate.AddYears(-1);
		ShownDate = target < MinimumDate ? MinimumDate : target;
		RaiseMonthChanged(oldShownDate);
	}

	// MinimumDate.Year is at least DateTime.MinValue.Year, so this also keeps AddYears in range.
	bool CanExecutePrevYear(object obj) => ShownDate.Year > MinimumDate.Year;

	/// <summary>
	/// Re-evaluates whether the arrows can be used, after a change of the shown date, of the allowed
	/// dates or of the layout.
	/// </summary>
	void RefreshNavigationCommands()
	{
		((Command)PrevLayoutUnitCommand).ChangeCanExecute();
		((Command)NextLayoutUnitCommand).ChangeCanExecute();
		((Command)PrevYearCommand).ChangeCanExecute();
		((Command)NextYearCommand).ChangeCanExecute();
	}

	/// <summary>
	/// Raises <see cref="MonthChanged"/> and executes <see cref="MonthChangedCommand"/> after the user
	/// moved the calendar from <paramref name="oldShownDate"/> to <see cref="ShownDate"/>.
	/// </summary>
	void RaiseMonthChanged(DateTime oldShownDate)
	{
		var args = new MonthChangedEventArgs(DateOnly.FromDateTime(oldShownDate), DateOnly.FromDateTime(ShownDate));
		MonthChanged?.Invoke(this, args);

		if (MonthChangedCommand?.CanExecute(args) == true)
		{
			MonthChangedCommand.Execute(args);
		}
	}

	void ToggleCalendarSectionVisibility() => CalendarSectionShown = !CalendarSectionShown;

	void AnimateMonths(double currentValue)
	{
		calendarContainer.HeightRequest = calendarSectionHeight * currentValue;
		calendarContainer.TranslationY = calendarSectionHeight * (currentValue - 1);
		calendarContainer.Opacity = currentValue * currentValue * currentValue;
	}

	public void ClearSelection()
	{
		isSelectingDates = false;
		SelectedDates = null;
		SelectedDate = null;
	}

	// The calendar only adds recognizers for these four directions (see UpdateSwipeGestures), and so do
	// the day cells that hand their swipes to the calendar on Android (see DayView).
	internal void OnSwiped(object sender, SwipedEventArgs e)
	{
		var swiped = e.Direction switch
		{
			SwipeDirection.Left => SwipedLeft,
			SwipeDirection.Right => SwipedRight,
			SwipeDirection.Up => SwipedUp,
			_ => SwipedDown,
		};

		swiped?.Invoke(this, EventArgs.Empty);
	}



	public void InitializeViewLayoutEngine()
	{
		CurrentViewLayoutEngine = new MonthViewEngine(FirstDayOfWeek);
	}


	void RenderLayout()
	{
		CurrentViewLayoutEngine = CalendarLayout switch
		{
			WeekLayout.Week => new WeekViewEngine(1, FirstDayOfWeek),
			WeekLayout.TwoWeek => new WeekViewEngine(2, FirstDayOfWeek),
			_ => new MonthViewEngine(FirstDayOfWeek),
		};

		daysControl.Children.Clear();
		daysControl.RowDefinitions.Clear();
		daysControl.ColumnDefinitions.Clear();

		// Item 3: GenerateLayout now populates daysControl directly, eliminating the
		// intermediate Grid allocation and the O(n) copy loops.
		CurrentViewLayoutEngine.GenerateLayout(
			daysControl,
			dayViews,
			this,
			DayTappedCommand,
			DayViewTemplate
		);

		dayTitleLabels = [.. daysControl.Children.OfType<Label>()];

		UpdateDayGlobalProperties();
		UpdateDayTitles();
		UpdateDays();

		// The cells join the grid only now that their models hold real data. On a calendar that is
		// already on screen they are rendered (and their DayViewTemplate content, including a
		// selector's choice, is created) once, for the right day, instead of first for empty models.
		foreach (var dayView in dayViews)
		{
			daysControl.Add(dayView);
		}

		UpdateWeekendBackground();

		// The layout engine decides how far the arrows move.
		RefreshNavigationCommands();
	}

	internal void AssignIndicatorColors(ref DayModel dayModel)
	{
		dayModel.EventIndicatorColor = EventIndicatorColor;
		dayModel.EventIndicatorSelectedColor = EventIndicatorSelectedColor;
		dayModel.EventIndicatorTextColor = EventIndicatorTextColor;
		dayModel.EventIndicatorSelectedTextColor = EventIndicatorSelectedTextColor;

		if (Events.TryGetValue(dayModel.Date, out var dayEventCollection))
		{
			if (dayEventCollection is IPersonalizableDayEvent personalizableDay)
			{
				dayModel.EventIndicatorColor =
					personalizableDay.EventIndicatorColor ?? EventIndicatorColor;
				dayModel.EventIndicatorSelectedColor =
					personalizableDay.EventIndicatorSelectedColor
				 ?? personalizableDay.EventIndicatorColor
				 ?? EventIndicatorSelectedColor;
				dayModel.EventIndicatorTextColor =
					personalizableDay.EventIndicatorTextColor ?? EventIndicatorTextColor;
				dayModel.EventIndicatorSelectedTextColor =
					personalizableDay.EventIndicatorSelectedTextColor
				 ?? personalizableDay.EventIndicatorTextColor
				 ?? EventIndicatorSelectedTextColor;
			}
			// A multi-event day that provides no colors still shows the single indicator dot.
			if (dayEventCollection is IMultiEventDay { Colors.Count: > 0 } multiEventDay)
			{
				SetEventColors(dayModel, [.. multiEventDay.Colors.Take(5)]);
			}
			else
			{
				SetEventColors(dayModel, [dayModel.IsSelected ? dayModel.EventIndicatorSelectedColor : dayModel.EventIndicatorColor]);
			}

			dayModel.EventCount = dayEventCollection?.Count ?? 0;
			SetEvents(dayModel, dayEventCollection);
		}
		else
		{
			SetEventColors(dayModel, []);
			dayModel.EventCount = 0;
			SetEvents(dayModel, null);
		}
	}

	// Events is a snapshot, so a template sees items added before the entry was assigned again.
	// It is replaced only when the items differ, for the same reason as SetEventColors.
	static void SetEvents(DayModel dayModel, ICollection dayEventCollection)
	{
		if (dayEventCollection is null || dayEventCollection.Count == 0)
		{
			if (dayModel.Events.Count > 0)
			{
				dayModel.Events = [];
			}
			return;
		}

		if (dayModel.Events.Count == dayEventCollection.Count
			&& dayModel.Events.SequenceEqual(dayEventCollection.Cast<object>()))
		{
			return;
		}

		var snapshot = new List<object>(dayEventCollection.Count);
		foreach (var item in dayEventCollection)
		{
			snapshot.Add(item);
		}

		dayModel.Events = snapshot;
	}

	// Every day update builds a new list, and the generated setter compares lists by reference,
	// so without this check every event day would raise PropertyChanged for EventColors on every
	// pass and make the event dots (a BindableLayout) be rebuilt although nothing changed.
	static void SetEventColors(DayModel dayModel, IReadOnlyList<Color> colors)
	{
		if (dayModel.EventColors is { } current && current.SequenceEqual(colors))
		{
			return;
		}

		dayModel.EventColors = colors;
	}

	void InitializeSelectionType()
	{
		CurrentSelectionEngine = new SingleSelectionEngine();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Events.CollectionChanged -= OnEventsCollectionChanged;
			StopTodayRefresh();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
