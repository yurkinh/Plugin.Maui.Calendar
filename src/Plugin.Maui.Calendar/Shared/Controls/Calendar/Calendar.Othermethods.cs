using System.Globalization;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	int GetWeekNumber(DateTime date)
	{
		return Culture.Calendar.GetWeekOfYear(
			date,
			CalendarWeekRule.FirstFourDayWeek,
			Culture.DateTimeFormat.FirstDayOfWeek
		);
	}

	void PrevUnit()
	{
		var oldMonth = DateOnly.FromDateTime(ShownDate);
		ShownDate = CurrentViewLayoutEngine.GetPreviousUnit(ShownDate);
		var newMonth = DateOnly.FromDateTime(ShownDate);

		var args = new MonthChangedEventArgs(oldMonth, newMonth);
		MonthChanged?.Invoke(this, args);

		if (MonthChangedCommand?.CanExecute(null) == true)
		{
			MonthChangedCommand.Execute(args);
		}
	}

	void NextUnit()
	{
		var oldMonth = DateOnly.FromDateTime(ShownDate);
		ShownDate = CurrentViewLayoutEngine.GetNextUnit(ShownDate);
		var newMonth = DateOnly.FromDateTime(ShownDate);

		var args = new MonthChangedEventArgs(oldMonth, newMonth);
		MonthChanged?.Invoke(this, args);

		if (MonthChangedCommand?.CanExecute(null) == true)
		{
			MonthChangedCommand.Execute(args);
		}
	}

	void NextYear(object obj)
	{
		ShownDate = ShownDate.AddYears(1);
	}

	bool CanExecuteNextYear(object obj)
	{
		try
		{
			var maxDate = Culture.Calendar.MaxSupportedDateTime;
			return ShownDate.Year < maxDate.Year;
		}
		catch
		{
			return false;
		}
	}

	void PrevYear(object obj)
	{
		ShownDate = ShownDate.AddYears(-1);
	}

	bool CanExecutePrevYear(object obj)
	{
		try
		{
			var minDate = Culture.Calendar.MinSupportedDateTime;
			return ShownDate.Year > minDate.Year;
		}
		catch
		{
			return false;
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

	void OnSwiped(object sender, SwipedEventArgs e)
	{
		switch (e.Direction)
		{
			case SwipeDirection.Left:
				OnSwipeLeft();
				break;
			case SwipeDirection.Right:
				OnSwipeRight();
				break;
			case SwipeDirection.Up:
				OnSwipeUp();
				break;
			case SwipeDirection.Down:
				OnSwipeDown();
				break;
		}
	}

	void OnSwipeLeft() => SwipedLeft?.Invoke(this, EventArgs.Empty);

	void OnSwipeRight() => SwipedRight?.Invoke(this, EventArgs.Empty);

	void OnSwipeUp() => SwipedUp?.Invoke(this, EventArgs.Empty);

	void OnSwipeDown() => SwipedDown?.Invoke(this, EventArgs.Empty);



	public void InitializeViewLayoutEngine()
	{
		CurrentViewLayoutEngine = new MonthViewEngine(FirstDayOfWeek);
	}


	void RenderLayout()
	{
		// Item 16: skip during construction; the constructor performs one render at the end.
		if (isInitializing)
		{
			return;
		}

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
			nameof(DaysTitleLabelStyle),
			DayTappedCommand
		);

		// Item 13: cache the 7 day-of-week header Labels so UpdateDayTitles doesn't
		// re-filter Children.OfType<Label>() on every culture/style change.
		dayTitleLabels = daysControl.Children.OfType<Label>().ToArray();

		// Item 2: push global properties onto the freshly created DayModels before the
		// per-day date render so UpdateDays only handles date-specific values.
		UpdateDayGlobalProperties();
		UpdateDayTitles();
		UpdateDays();
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
					personalizableDay?.EventIndicatorColor ?? EventIndicatorColor;
				dayModel.EventIndicatorSelectedColor =
					personalizableDay?.EventIndicatorSelectedColor
				 ?? personalizableDay?.EventIndicatorColor
				 ?? EventIndicatorSelectedColor;
				dayModel.EventIndicatorTextColor =
					personalizableDay?.EventIndicatorTextColor ?? EventIndicatorTextColor;
				dayModel.EventIndicatorSelectedTextColor =
					personalizableDay?.EventIndicatorSelectedTextColor
				 ?? personalizableDay?.EventIndicatorTextColor
				 ?? EventIndicatorSelectedTextColor;
			}
			if (dayEventCollection is IMultiEventDay multiEventDay)
			{
				dayModel.EventColors = multiEventDay.Colors?.Take(5).ToList() ?? [];
			}
			else
			{
				dayModel.EventColors = [dayModel.IsSelected ? dayModel.EventIndicatorSelectedColor : dayModel.EventIndicatorColor];
			}
		}
		else
		{
			dayModel.EventColors = [];
		}
	}

	void InitializeSelectionType()
	{
		CurrentSelectionEngine = new SingleSelectionEngine();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Events is EventCollection events)
			{
				events.CollectionChanged -= OnEventsCollectionChanged;
			}
			calendarSectionAnimateHide.Value.Dispose();
			calendarSectionAnimateShow.Value.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
