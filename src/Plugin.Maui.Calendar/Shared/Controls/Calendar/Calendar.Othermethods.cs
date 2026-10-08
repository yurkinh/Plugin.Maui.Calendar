using System.Globalization;
using Plugin.Maui.Calendar.Controls.Drawn;
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

	void OnSwiped(object sender, SwipedEventArgs e) => HandleSwipe(e.Direction);

	void OnDaysGridSwiped(object sender, SwipeDirection direction)
	{
		if (!SwipeDetectionDisabled)
		{
			HandleSwipe(direction);
		}
	}

	void HandleSwipe(SwipeDirection direction)
	{
		// the same physical swipe may reach both the canvas and the native recognizers
		var now = DateTime.UtcNow;
		if (direction == lastSwipeDirection && now - lastSwipeTime < swipeDedupWindow)
		{
			return;
		}
		lastSwipeDirection = direction;
		lastSwipeTime = now;

		switch (direction)
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

		if (daysGrid is null)
		{
			daysGrid = new DaysGrid();
			daysGrid.Swiped += OnDaysGridSwiped;
			daysCanvas.Content = daysGrid;
		}

		// Item 3: GenerateLayout rebuilds the drawn grid in place.
		CurrentViewLayoutEngine.GenerateLayout(daysGrid, dayModels, DayTappedCommand);

		// Item 2: push global properties onto the freshly created DayModels before the
		// per-day date render so UpdateDays only handles date-specific values.
		ApplyDaysLabelStyle();
		UpdateDayGlobalProperties();
		UpdateDayTitles();
		// fresh models: render even when the first shown date did not change
		UpdateDays(true);
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
