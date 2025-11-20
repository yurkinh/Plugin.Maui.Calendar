using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;
using System.Collections.ObjectModel;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	void UpdateEvents()
	{
		SelectedDayEvents = CurrentSelectionEngine.TryGetSelectedEvents(Events, out var selectedEvents) ? selectedEvents : null;

		eventsScrollView.ScrollToAsync(0, 0, false);
	}

	void UpdateLayoutUnitLabel()
	{
		if (WeekViewUnit == WeekViewUnit.WeekNumber)
		{
			LayoutUnitText = GetWeekNumber(ShownDate).ToString();
			return;
		}

		LayoutUnitText = Culture.DateTimeFormat.MonthNames[ShownDate.Month - 1].Capitalize();
	}

	void UpdateSelectedDateLabel() => SelectedDateText = CurrentSelectionEngine.GetSelectedDateText(SelectedDateTextFormat, Culture, UseNativeDigits);

	void ShowHideCalendarSection()
	{
		if (calendarSectionAnimating)
		{
			return;
		}

		calendarSectionAnimating = true;

		var animation = CalendarSectionShown ? calendarSectionAnimateShow : calendarSectionAnimateHide;
		var prevState = CalendarSectionShown;

		animation.Value.Commit(
			this,
			calendarSectionAnimationId,
			calendarSectionAnimationRate,
			calendarSectionAnimationDuration,
			finished: (value, cancelled) =>
			{
				calendarSectionAnimating = false;

				if (prevState != CalendarSectionShown)
				{
					ToggleCalendarSectionVisibility();
				}
			}
		);
	}

	void UpdateCalendarSectionHeight()
	{
		calendarSectionHeight = calendarContainer.Height;
	}

	void OnEventsCollectionChanged(object sender, EventCollection.EventCollectionChangedArgs e)
	{
		UpdateEvents();
		UpdateDays();
		UpdateDaysColors();
	}

	void OnDayTappedHandler(DateTime value)
	{
		if (AutoChangeMonthOnDayTap)
		{
			if (value.Month != ShownDate.Month || value.Year != ShownDate.Year)
			{
				var oldMonth = new DateOnly(ShownDate.Year, ShownDate.Month, 1);
				var newMonth = new DateOnly(value.Year, value.Month, 1);

				ShownDate = value;

				MonthChanged?.Invoke(this, new MonthChangedEventArgs(oldMonth, newMonth));

				if (MonthChangedCommand?.CanExecute(null) == true)
				{
					MonthChangedCommand.Execute(new MonthChangedEventArgs(oldMonth, newMonth));
				}
			}
		}

		SelectedDates = new ObservableCollection<DateTime>(CurrentSelectionEngine.PerformDateSelection(value, DisabledDates));
	}

	void UpdateDayTitles()
	{
		var dayNumber = (int)FirstDayOfWeek;

		foreach (var dayLabel in daysControl.Children.OfType<Label>())
		{
			string dayName;
			if (UseAbbreviatedDayNames)
			{
				dayName = Culture.DateTimeFormat.AbbreviatedDayNames[dayNumber];
			}
			else
			{
				var fullName = Culture.DateTimeFormat.DayNames[dayNumber];
				dayName = DaysTitleMaximumLength == DaysTitleMaxLength.None
						? fullName
						: fullName[..((int)DaysTitleMaximumLength > fullName.Length ? fullName.Length : (int)DaysTitleMaximumLength)];
			}

			var titleText = DaysTitleLabelFirstUpperRestLower
							? dayName[..1].ToUpperInvariant() + dayName[1..].ToLowerInvariant()
							: dayName.ToUpperInvariant();

			dayLabel.Text = titleText;

			// Detect weekend days	
			if (dayNumber == (int)DayOfWeek.Saturday || dayNumber == (int)DayOfWeek.Sunday)
			{
				dayLabel.Style = WeekendTitleStyle;
			}
			dayNumber = (dayNumber + 1) % 7;
		}
	}

	DateTime firstDate = DateTime.MinValue;
	void UpdateDays(bool forceUpdate = false)
	{
		int lastDayOfMonth = 0;
		if (!forceUpdate && firstDate == CurrentViewLayoutEngine.GetFirstDate(ShownDate))
		{
			return;
		}
		firstDate = CurrentViewLayoutEngine.GetFirstDate(ShownDate);

		int addDays = 0;
		var remainingDaysUntilMax = (DateTime.MaxValue.Date - firstDate.Date).Days + 1;
		var safeOffsets = (int)Math.Min(dayViews.Count, Math.Max(0, remainingDaysUntilMax));

		foreach (var dayView in dayViews)
		{
			var dayModel = dayView.BindingContext as DayModel;

			if (addDays < safeOffsets)
			{
				var currentDate = firstDate.AddDays(addDays++);

				if (currentDate.Month == ShownDate.Month)
				{
					lastDayOfMonth = addDays;
				}

				bool currentMonthOnLine = lastDayOfMonth == 0 || (addDays - 1) / 7 == (lastDayOfMonth - 1) / 7;

				dayModel.Date = currentDate.Date;
				dayModel.Day = UseNativeDigits ? currentDate.Day.ToNativeDigitString(Culture) : currentDate.Day.ToString(Culture);
				dayModel.DayTappedCommand = DayTappedCommand;
				dayModel.EventIndicatorType = EventIndicatorType;
				dayModel.DayViewSize = DayViewSize;
				dayModel.DayViewBorderMargin = DayViewBorderMargin;
				dayModel.DayViewCornerRadius = DayViewCornerRadius;
				dayModel.DaysLabelStyle = DaysLabelStyle;
				dayModel.IsThisMonth = CalendarLayout != WeekLayout.Month || currentDate.Month == ShownDate.Month;
				dayModel.OtherMonthIsVisible = CalendarLayout != WeekLayout.Month || OtherMonthDayIsVisible;
				dayModel.OtherMonthWeekIsVisible = CalendarLayout != WeekLayout.Month || OtherMonthWeekIsVisible || (OtherMonthDayIsVisible && currentMonthOnLine);
				dayModel.HasEvents = Events.ContainsKey(currentDate);
				dayModel.IsDisabled = currentDate < MinimumDate || currentDate > MaximumDate || (DisabledDates?.Contains(currentDate.Date) ?? false);
				dayModel.AllowDeselect = AllowDeselecting;

				dayModel.IsSelected = CurrentSelectionEngine.IsDateSelected(dayModel.Date);
				AssignIndicatorColors(ref dayModel);
			}
			else
			{
				addDays++;

				dayModel.Date = DateTime.MaxValue.Date;
				dayModel.Day = string.Empty;
				dayModel.DayTappedCommand = DayTappedCommand;
				dayModel.EventIndicatorType = EventIndicatorType;
				dayModel.DayViewSize = DayViewSize;
				dayModel.DayViewBorderMargin = DayViewBorderMargin;
				dayModel.DayViewCornerRadius = DayViewCornerRadius;
				dayModel.DaysLabelStyle = DaysLabelStyle;
				dayModel.IsThisMonth = false;
				dayModel.OtherMonthIsVisible = false;
				dayModel.OtherMonthWeekIsVisible = false;
				dayModel.HasEvents = false;
				dayModel.IsDisabled = true;
				dayModel.AllowDeselect = AllowDeselecting;
				dayModel.IsSelected = false;
				AssignIndicatorColors(ref dayModel);
			}
		}
	}

	void UpdateDaysColors()
	{
		foreach (var dayView in dayViews)
		{
			var dayModel = dayView.BindingContext as DayModel;

			dayModel.DeselectedTextColor = DeselectedDayTextColor;
			dayModel.TodayTextColor = TodayTextColor;
			dayModel.SelectedTextColor = SelectedDayTextColor;
			dayModel.SelectedTodayTextColor = SelectedTodayTextColor;
			dayModel.OtherMonthColor = OtherMonthDayColor;
			dayModel.OtherMonthSelectedColor = OtherMonthSelectedDayColor;
			dayModel.WeekendDayColor = WeekendDayColor;
			dayModel.SelectedBackgroundColor = SelectedDayBackgroundColor;
			dayModel.TodayOutlineColor = TodayOutlineColor;
			dayModel.TodayFillColor = TodayFillColor;
			dayModel.DisabledColor = DisabledDayColor;

			AssignIndicatorColors(ref dayModel);
		}
	}
}
