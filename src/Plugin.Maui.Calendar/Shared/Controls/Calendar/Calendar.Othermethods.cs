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
	readonly Dictionary<int, BoxView> weekSeparators = new();
	BoxView headerTitlesSeparator;

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

		MonthChanged?.Invoke(this, new MonthChangedEventArgs(oldMonth, newMonth));

		if (MonthChangedCommand?.CanExecute(null) == true)
		{
			MonthChangedCommand.Execute(new MonthChangedEventArgs(oldMonth, newMonth));
		}
	}

	void NextUnit()
	{
		var oldMonth = DateOnly.FromDateTime(ShownDate);
		ShownDate = CurrentViewLayoutEngine.GetNextUnit(ShownDate);
		var newMonth = DateOnly.FromDateTime(ShownDate);

		MonthChanged?.Invoke(this, new MonthChangedEventArgs(oldMonth, newMonth));

		if (MonthChangedCommand?.CanExecute(null) == true)
		{
			MonthChangedCommand.Execute(new MonthChangedEventArgs(oldMonth, newMonth));
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
		CurrentViewLayoutEngine = CalendarLayout switch
		{
			WeekLayout.Week => new WeekViewEngine(1, FirstDayOfWeek),
			WeekLayout.TwoWeek => new WeekViewEngine(2, FirstDayOfWeek),
			_ => new MonthViewEngine(FirstDayOfWeek),
		};

		daysControl.Children.Clear();
		daysControl.RowDefinitions.Clear();
		daysControl.ColumnDefinitions.Clear();
		weekSeparators.Clear();

		var generatedLayout = CurrentViewLayoutEngine.GenerateLayout(
			dayViews,
			this,
			nameof(DaysTitleLabelStyle),
			DayTappedCommand
		);

		foreach (var colDef in generatedLayout.ColumnDefinitions)
		{
			daysControl.ColumnDefinitions.Add(colDef);
		}

		for (int i = 0; i < generatedLayout.RowDefinitions.Count; i++)
		{
			const int columnsCount = 7;

			if (i == 0)
			{
				var headerTitlesBackground = new Border { Style = HeaderTitlesBackgroundStyle };
				Grid.SetRow(headerTitlesBackground, i);
				Grid.SetColumnSpan(headerTitlesBackground, columnsCount);
				daysControl.Children.Add(headerTitlesBackground);

				headerTitlesSeparator = new BoxView
				{
					Style = HeaderTitlesSeparatorStyle,
					IsVisible = HeaderTitlesSeparatorIsVisible
				};
				Grid.SetRow(headerTitlesSeparator, i);
				Grid.SetColumnSpan(headerTitlesSeparator, columnsCount);
				daysControl.Children.Add(headerTitlesSeparator);
			}

			if (i > 0 && SeparatorIsVisible)
			{
				var separator = new BoxView
				{
					Style = SeparatorStyle,
					ClassId = $"week_separator_{i}"
				};
				Grid.SetRow(separator, i);
				Grid.SetColumnSpan(separator, columnsCount);
				daysControl.Children.Add(separator);
				weekSeparators[i] = separator;
			}

			daysControl.RowDefinitions.Add(generatedLayout.RowDefinitions[i]);
		}

		foreach (var child in generatedLayout.Children)
		{
			daysControl.Children.Add(child);
		}

		UpdateDaysColors();
		UpdateDayTitles();
		UpdateDays();
	}

	void UpdateSeparatorVisibility()
	{
		if (!SeparatorIsVisible || weekSeparators.Count == 0)
		{
			return;
		}

		foreach (var (rowIndex, separator) in weekSeparators)
		{
			int startIndex = (rowIndex - 1) * 7;
			if (startIndex < 0 || startIndex >= dayViews.Count)
			{
				continue;
			}

			bool rowIsEmpty = dayViews
				.Skip(startIndex)
				.Take(7)
				.All(dv => dv.BindingContext is DayModel dm && !dm.IsControlVisible);

			separator.IsVisible = !rowIsEmpty;
		}
	}

	internal void AssignEventIndicatorColors(ref DayModel dayModel)
	{
		SyncDayModelStyles(dayModel);

		if (Events.TryGetValue(dayModel.Date, out var dayEventCollection) && dayEventCollection is IEnumerable collection)
		{
			var enumerator = collection.GetEnumerator();
			bool hasAnyItem = enumerator.MoveNext();

			dayModel.HasEvents = hasAnyItem;

			if (hasAnyItem)
			{
				dayModel.EventIndicators = ResolveEventIndicators(dayEventCollection);
			}
			else
			{
				dayModel.EventIndicators = [];
			}
		}
		else
		{
			dayModel.HasEvents = false;
			dayModel.EventIndicators = [];
		}
	}

	internal void SyncDayModelStyles(DayModel dayModel)
	{
		dayModel.DayViewSize = DayViewSize;
		dayModel.EventDayBackgroundColorIsActive = EventDayBackgroundColorIsActive;
		dayModel.EventDayBackgroundColor = EventDayBackgroundColor;
		dayModel.DayViewBorderMargin = DayViewBorderMargin;
		dayModel.DayViewCornerRadius = DayViewCornerRadius;
		dayModel.EventIndicatorDotStyle = EventIndicatorDotStyle;
		dayModel.EventIndicatorTextContainerStyle = EventIndicatorTextContainerStyle;
		dayModel.EventIndicatorTextStyle = EventIndicatorTextStyle;
		dayModel.EventIndicatorImageStyle = EventIndicatorImageStyle;
	}

	internal List<EventIndicatorModel> ResolveEventIndicators(object dayEventCollection)
	{
		if (dayEventCollection is IMultiEventDay { EventIndicators.Count: > 0 } multiEventDay)
		{
			return multiEventDay.EventIndicators
				.Take(5)
				.Select(e => new EventIndicatorModel
				{
					DotColor = e.DotColor,
					Text = e.Text,
					ImageSource = e.ImageSource,
				}).ToList();
		}

		if (dayEventCollection is IPersonalizableDayEvent { EventIndicator: not null } personalizableDay)
		{
			return [personalizableDay.EventIndicator];
		}

		return
		[
			new EventIndicatorModel { DotColor = EventIndicatorDefaultColor ?? Colors.DeepPink }
		];
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