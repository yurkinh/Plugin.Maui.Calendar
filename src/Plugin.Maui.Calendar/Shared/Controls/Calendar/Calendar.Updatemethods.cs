using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Shared.Extensions;
using System.Collections.ObjectModel;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	void UpdateEvents()
	{
		if (isInitializing)
		{
			return;
		}

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
		// Item 1: UpdateDays already calls AssignIndicatorColors per day, so a separate
		// UpdateDaysColors pass would be a redundant second iteration.
		UpdateEvents();
		UpdateDays();
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

				// Item 6: construct MonthChangedEventArgs once and reuse for both the
				// event and the command to avoid a second allocation.
				var args = new MonthChangedEventArgs(oldMonth, newMonth);
				MonthChanged?.Invoke(this, args);

				if (MonthChangedCommand?.CanExecute(null) == true)
				{
					MonthChangedCommand.Execute(args);
				}
			}
		}

		SelectedDates = new ObservableCollection<DateTime>(CurrentSelectionEngine.PerformDateSelection(value, DisabledDates));
	}

	// Item 13: the 7 day-of-week title labels are created by the drawn grid and read
	// from it directly.
	void UpdateDayTitles()
	{
		if (daysGrid is null)
		{
			return;
		}

		var dayNumber = (int)FirstDayOfWeek;

		foreach (var dayLabel in daysGrid.TitleLabels)
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
			dayNumber = (dayNumber + 1) % 7;
		}

		ApplyTitleStyles();
	}

	/// <summary>
	/// Applies the resolved <see cref="DaysTitleLabelStyle"/> to weekday titles and
	/// <see cref="WeekendTitleStyle"/> to Saturday/Sunday titles.
	/// </summary>
	void ApplyTitleStyles()
	{
		if (daysGrid is null || daysTitleStyleBridge is null)
		{
			return;
		}

		var dayNumber = (int)FirstDayOfWeek;
		foreach (var dayLabel in daysGrid.TitleLabels)
		{
			var isWeekend = dayNumber == (int)DayOfWeek.Saturday || dayNumber == (int)DayOfWeek.Sunday;
			var bridge = isWeekend ? weekendTitleStyleBridge : daysTitleStyleBridge;
			bridge.ApplyTo(dayLabel, applyTextColor: true, applyLayoutOptions: true);
			dayNumber = (dayNumber + 1) % 7;
		}
	}

	/// <summary>Applies the resolved <see cref="DaysLabelStyle"/> to every day label.</summary>
	void ApplyDaysLabelStyle()
	{
		if (daysGrid is null || daysLabelStyleBridge is null)
		{
			return;
		}

		foreach (var cell in daysGrid.Cells)
		{
			cell.ApplyLabelStyle(daysLabelStyleBridge);
		}
	}

	DateTime firstDate = DateTime.MinValue;
	void UpdateDays(bool forceUpdate = false)
	{
		// Item 16: skip all work during construction; one consolidated render fires at the
		// end of the Calendar() constructor.
		if (isInitializing)
		{
			return;
		}

		int lastDayOfMonth = 0;
		if (!forceUpdate && firstDate == CurrentViewLayoutEngine.GetFirstDate(ShownDate))
		{
			return;
		}
		firstDate = CurrentViewLayoutEngine.GetFirstDate(ShownDate);

		int addDays = 0;
		var remainingDaysUntilMax = (DateTime.MaxValue.Date - firstDate.Date).Days + 1;
		var safeOffsets = (int)Math.Min(dayModels.Count, Math.Max(0, remainingDaysUntilMax));

		// Item 4: build a HashSet<DateTime> once so each per-day IsDisabled check is O(1)
		// instead of O(n) with List.Contains.
		var disabledSet = DisabledDates?.Count > 0 ? new HashSet<DateTime>(DisabledDates) : null;

		for (int i = 0; i < dayModels.Count; i++)
		{
			var dayModel = dayModels[i];

			if (addDays < safeOffsets)
			{
				var currentDate = firstDate.AddDays(addDays++);

				if (currentDate.Month == ShownDate.Month)
				{
					lastDayOfMonth = addDays;
				}

				bool currentMonthOnLine = lastDayOfMonth == 0 || (addDays - 1) / 7 == (lastDayOfMonth - 1) / 7;

				// Item 2: only date-specific values are set here; global/color props are
				// propagated by UpdateDayGlobalProperties so they don't need to be pushed
				// on every date-change render.
				dayModel.Date = currentDate.Date;
				dayModel.Day = UseNativeDigits ? currentDate.Day.ToNativeDigitString(Culture) : currentDate.Day.ToString(Culture);
				dayModel.IsThisMonth = CalendarLayout != WeekLayout.Month || currentDate.Month == ShownDate.Month;
				dayModel.OtherMonthIsVisible = CalendarLayout != WeekLayout.Month || OtherMonthDayIsVisible;
				dayModel.OtherMonthWeekIsVisible = CalendarLayout != WeekLayout.Month || OtherMonthWeekIsVisible || (OtherMonthDayIsVisible && currentMonthOnLine);
				dayModel.HasEvents = Events.ContainsKey(currentDate);
				dayModel.IsDisabled = currentDate < MinimumDate || currentDate > MaximumDate || (disabledSet?.Contains(currentDate.Date) ?? false);
				dayModel.IsSelected = CurrentSelectionEngine.IsDateSelected(dayModel.Date);
				AssignIndicatorColors(ref dayModel);
			}
			else
			{
				addDays++;

				dayModel.Date = DateTime.MaxValue.Date;
				dayModel.Day = string.Empty;
				dayModel.IsThisMonth = false;
				dayModel.OtherMonthIsVisible = false;
				dayModel.OtherMonthWeekIsVisible = false;
				dayModel.HasEvents = false;
				dayModel.IsDisabled = true;
				dayModel.IsSelected = false;
				AssignIndicatorColors(ref dayModel);
			}
		}
	}

	/// <summary>
	/// Pushes all global (calendar-wide, not per-day) property values onto every
	/// <see cref="DayModel"/> in one pass.  This is called once after layout generation
	/// and again whenever a global property changes, so <see cref="UpdateDays"/> only
	/// needs to handle date-specific values.
	/// </summary>
	void UpdateDayGlobalProperties()
	{
		if (daysLabelStyleBridge is not null)
		{
			// raises Changed (-> ApplyDaysLabelStyle) only when the style instance changed
			daysLabelStyleBridge.Style = DaysLabelStyle;
		}

		if (daysGrid is not null)
		{
			foreach (var cell in daysGrid.Cells)
			{
				cell.Culture = Culture;
			}
		}

		for (int i = 0; i < dayModels.Count; i++)
		{
			var dayModel = dayModels[i];

			// Structural global props
			dayModel.DayTappedCommand = DayTappedCommand;
			dayModel.EventIndicatorType = EventIndicatorType;
			dayModel.DayViewSize = DayViewSize;
			dayModel.DayViewBorderMargin = DayViewBorderMargin;
			dayModel.DayViewCornerRadius = DayViewCornerRadius;
			dayModel.DaysLabelStyle = DaysLabelStyle;
			dayModel.AllowDeselect = AllowDeselecting;

			// Color global props
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

			// Indicator colors depend on per-day state (Events, IsSelected) so they must
			// be recomputed even in a color-only update.
			AssignIndicatorColors(ref dayModel);
		}
	}

	/// <summary>
	/// Updates day colors and event-indicator colors without recomputing date layout.
	/// Delegates to <see cref="UpdateDayGlobalProperties"/>, the single authoritative
	/// method for propagating all global properties to DayModels.
	/// </summary>
	void UpdateDaysColors() => UpdateDayGlobalProperties();
}
