using Microsoft.Maui.Controls.Shapes;
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

	// Item 13: cached references to the 7 day-of-week title labels created during
	// RenderLayout.  UpdateDayTitles iterates this array rather than calling
	// daysControl.Children.OfType<Label>() on every invocation.
	void UpdateDayTitles()
	{
		if (dayTitleLabels is null)
		{
			return;
		}

		var dayNumber = (int)FirstDayOfWeek;

		foreach (var dayLabel in dayTitleLabels)
		{
			var fullName = (UseAbbreviatedDayNames
				? Culture.DateTimeFormat.AbbreviatedDayNames[dayNumber]
				: Culture.DateTimeFormat.DayNames[dayNumber]).NormalizeDayName(Culture);
			var dayName = fullName;

			if (!UseAbbreviatedDayNames)
			{
				dayName = DaysTitleMaximumLength == DaysTitleMaxLength.None
						? fullName
						: fullName.TruncateDayName(Culture.DateTimeFormat.AbbreviatedDayNames[dayNumber].NormalizeDayName(Culture), (int)DaysTitleMaximumLength);
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
		var lastDate = CurrentViewLayoutEngine.GetLastDate(ShownDate);

		var shownDatesChanged = VisibleStartDate != firstDate.Date || VisibleEndDate != lastDate.Date;
		SetValue(VisibleStartDatePropertyKey, firstDate.Date);
		SetValue(VisibleEndDatePropertyKey, lastDate.Date);

		int addDays = 0;
		var remainingDaysUntilMax = (DateTime.MaxValue.Date - firstDate.Date).Days + 1;
		var safeOffsets = (int)Math.Min(dayViews.Count, Math.Max(0, remainingDaysUntilMax));

		// Item 4: build a HashSet<DateTime> once so each per-day IsDisabled check is O(1)
		// instead of O(n) with List.Contains.
		var disabledSet = DisabledDates?.Count > 0 ? new HashSet<DateTime>(DisabledDates) : null;

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

				// Item 2: only date-specific values are set here; global/color props are
				// propagated by UpdateDayGlobalProperties so they don't need to be pushed
				// on every date-change render.
				dayModel.Date = currentDate.Date;
				dayModel.Day = UseNativeDigits ? currentDate.Day.ToNativeDigitString(Culture) : currentDate.Day.ToString(Culture);
				dayModel.IsThisMonth = CalendarLayout != WeekLayout.Month || currentDate.Month == ShownDate.Month;
				dayModel.OtherMonthIsVisible = CalendarLayout != WeekLayout.Month || OtherMonthDayIsVisible;
				dayModel.OtherMonthWeekIsVisible = CalendarLayout != WeekLayout.Month || OtherMonthWeekIsVisible || (OtherMonthDayIsVisible && currentMonthOnLine);
				dayModel.HasEvents = Events.ContainsKey(currentDate);
				// Normalise to date-only so a non-midnight MinimumDate/MaximumDate
				// (e.g. DateTime.Now.AddDays(-7)) does not incorrectly disable the boundary day.
				dayModel.IsDisabled = IsDateDisabled(currentDate, MinimumDate, MaximumDate, disabledSet);
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

		if (shownDatesChanged)
		{
			var args = new ShownDatesChangedEventArgs(VisibleStartDate, VisibleEndDate);
			ShownDatesChanged?.Invoke(this, args);

			if (ShownDatesChangedCommand?.CanExecute(args) == true)
			{
				ShownDatesChangedCommand.Execute(args);
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
		foreach (var dayView in dayViews)
		{
			var dayModel = dayView.BindingContext as DayModel;
			if (dayModel is null)
			{
				continue;
			}

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
			dayModel.OtherMonthSelectedColor = OtherMonthSelectedDayTextColor;
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

	/// <summary>
	/// Synchronises the weekend-day background boxes with <see cref="WeekendDayBackgroundColor"/>
	/// and <see cref="WeekendDayBackgroundCornerRadius"/>. The boxes are created lazily and only
	/// while a visible (non-transparent) colour is set, so nothing is added to the visual tree
	/// when the feature is unused (the default). Called once per layout regeneration and
	/// whenever either property changes.
	/// </summary>
	internal void UpdateWeekendBackground()
	{
		// Start from a clean slate so a colour change can never leave stale boxes behind; they
		// are rebuilt below only when a visible colour is set.
		RemoveWeekendBands();

		if (WeekendDayBackgroundColor is not { Alpha: > 0 })
		{
			return;
		}

		weekendBackgroundBands = CreateWeekendBands();

		var cornerRadius = new CornerRadius(WeekendDayBackgroundCornerRadius);
		foreach (var band in weekendBackgroundBands)
		{
			band.BackgroundColor = WeekendDayBackgroundColor;
			band.StrokeShape = new RoundRectangle { CornerRadius = cornerRadius };
		}
	}

	/// <summary>
	/// Creates one input-transparent <see cref="Border"/> behind every weekend day cell (week
	/// rows only, so the day-of-week title row stays uncovered) and inserts them at the front
	/// of <c>daysControl</c> so they render behind the day cells. Each box bleeds half the row
	/// spacing above and below (a negative vertical margin) so vertically-consecutive weekend
	/// days touch with no gap. Weekend columns are derived from <see cref="FirstDayOfWeek"/>.
	/// </summary>
	Border[] CreateWeekendBands()
	{
		const int daysInWeek = 7;

		int rowCount = daysControl.RowDefinitions.Count;
		if (rowCount <= 1)
		{
			// Grid not built yet (or header only) — nothing to place boxes behind.
			return [];
		}

		int numberOfWeeks = rowCount - 1;
		double verticalBleed = daysControl.RowSpacing / 2d;

		var bands = new List<Border>();
		for (int col = 0; col < daysInWeek; col++)
		{
			if (!ViewLayoutEngines.ViewLayoutBase.IsWeekendColumn(FirstDayOfWeek, col))
			{
				continue;
			}

			for (int week = 1; week <= numberOfWeeks; week++)
			{
				var band = new Border
				{
					BackgroundColor = Colors.Transparent,
					Stroke = Colors.Transparent,
					StrokeThickness = 0,
					Padding = 0,
					Margin = new Thickness(0, -verticalBleed, 0, -verticalBleed),
					InputTransparent = true,
				};
				Grid.SetColumn(band, col);
				Grid.SetRow(band, week);

				// Insert at the front so the box sits behind the titles and day cells.
				daysControl.Children.Insert(bands.Count, band);
				bands.Add(band);
			}
		}

		return [.. bands];
	}

	/// <summary>
	/// Detaches and forgets any weekend background boxes previously added to <c>daysControl</c>.
	/// </summary>
	void RemoveWeekendBands()
	{
		if (weekendBackgroundBands is null)
		{
			return;
		}

		foreach (var band in weekendBackgroundBands)
		{
			daysControl.Children.Remove(band);
		}

		weekendBackgroundBands = null;
	}

	/// <summary>
	/// Returns <see langword="true"/> when <paramref name="currentDate"/> falls outside the
	/// [<paramref name="minimumDate"/>, <paramref name="maximumDate"/>] range or is present
	/// in <paramref name="disabledSet"/>.
	/// </summary>
	/// <remarks>
	/// Both bounds are compared using <see cref="DateTime.Date"/> so that a caller who
	/// supplies <c>DateTime.Now.AddDays(-7)</c> (which carries a non-midnight time) does
	/// not accidentally disable the boundary calendar day.
	/// </remarks>
	internal static bool IsDateDisabled(
		DateTime currentDate,
		DateTime minimumDate,
		DateTime maximumDate,
		HashSet<DateTime> disabledSet)
		=> currentDate.Date < minimumDate.Date
		|| currentDate.Date > maximumDate.Date
		|| (disabledSet?.Contains(currentDate.Date) ?? false);
}
