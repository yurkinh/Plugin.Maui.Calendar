using System.Collections.ObjectModel;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;


public class RangeSelectionCalendar : Calendar
{
	/// <summary> Bindable property for StartDate </summary>
	public static readonly BindableProperty SelectedStartDateProperty = BindableProperty.Create(
		nameof(SelectedStartDate),
		typeof(DateTime?),
		typeof(RangeSelectionCalendar),
		null,
		BindingMode.TwoWay,
		propertyChanged: OnSelectedStartDateChanged
	);

	/// <summary>
	/// Beggining of selected interval
	/// </summary>
	public DateTime? SelectedStartDate
	{
		get => (DateTime?)GetValue(SelectedStartDateProperty);
		set => SetValue(SelectedStartDateProperty, value);
	}

	/// <summary> Bindable property for EndDate </summary>
	public static readonly BindableProperty SelectedEndDateProperty = BindableProperty.Create(
		nameof(SelectedEndDate),
		typeof(DateTime?),
		typeof(RangeSelectionCalendar),
		null,
		BindingMode.TwoWay,
		propertyChanged: OnSelectedEndDateChanged
	);

	/// <summary> End of selected interval </summary>
	public DateTime? SelectedEndDate
	{
		get => (DateTime?)GetValue(SelectedEndDateProperty);
		set => SetValue(SelectedEndDateProperty, value);
	}

	/// <summary>
	/// Background color for the range between SelectedStartDate and SelectedEndDate.
	/// </summary>
	public static readonly BindableProperty SelectedDatesRangeBackgroundColorProperty = BindableProperty.Create(
		nameof(SelectedDatesRangeBackgroundColor),
		typeof(Color),
		typeof(RangeSelectionCalendar),
		null,
		propertyChanged: static (bindable, oldValue, newValue) => ((RangeSelectionCalendar)bindable).UpdateDateColors()
	);

	/// <summary>
	/// Background color for the range between SelectedStartDate and SelectedEndDate.
	/// </summary>
	public Color SelectedDatesRangeBackgroundColor
	{
		get
		{
			var color = (Color)GetValue(SelectedDatesRangeBackgroundColorProperty);

			if (color == null)
			{
				return SelectedDayBackgroundColor;
			}

			return color;
		}
		set => SetValue(SelectedDatesRangeBackgroundColorProperty, value);
	}
	bool isSelectionDatesChanging = false;
	readonly RangedSelectionEngine selectionEngine;

	/// <summary>
	/// Constructor
	/// </summary>
	public RangeSelectionCalendar() : base()
	{
		CurrentSelectionEngine = new RangedSelectionEngine();
		selectionEngine = CurrentSelectionEngine as RangedSelectionEngine;
	}

	protected override void UpdateRangeSelection()
	{
		var first = selectionEngine.GetDateRange(DisabledDates);

		if (first.Count > 0)
		{
			isSelectionDatesChanging = true;
			SelectedStartDate = first.FirstOrDefault();
			SelectedEndDate = first.LastOrDefault();
			if (!first.Select(d => d.Date).SequenceEqual(SelectedDates.Select(d => d.Date)))
			{
				SelectedDates = new ObservableCollection<DateTime>(first);
			}

			// OnSelectedEndDateChanged only clears the flag when SelectedEndDate actually changed;
			// a range extended at its start keeps its end, which left the flag set and made the
			// next programmatic SelectedStartDate/SelectedEndDate assignment be ignored.
			isSelectionDatesChanging = false;
		}

		UpdateDateColors();
	}

	static void OnSelectedStartDateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var rangeSelectionCalendar = (RangeSelectionCalendar)bindable;
		if (!rangeSelectionCalendar.isSelectionDatesChanging)
		{
			rangeSelectionCalendar.isSelectionDatesChanging = true;
			rangeSelectionCalendar.selectionEngine.SelectDateRange((DateTime?)newValue, rangeSelectionCalendar.DisabledDates);
			rangeSelectionCalendar.SelectedDates = new ObservableCollection<DateTime>(
				rangeSelectionCalendar.selectionEngine.GetDateRange(rangeSelectionCalendar.DisabledDates)
			);
			rangeSelectionCalendar.isSelectionDatesChanging = false;
		}
		rangeSelectionCalendar.UpdateDateColors();
	}

	static void OnSelectedEndDateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var rangeSelectionCalendar = (RangeSelectionCalendar)bindable;
		if (!rangeSelectionCalendar.isSelectionDatesChanging)
		{
			rangeSelectionCalendar.isSelectionDatesChanging = true;
			rangeSelectionCalendar.selectionEngine.SelectDateRange((DateTime?)newValue, rangeSelectionCalendar.DisabledDates);
			rangeSelectionCalendar.SelectedDates = new ObservableCollection<DateTime>(
				rangeSelectionCalendar.selectionEngine.GetDateRange()
			);
		}
		rangeSelectionCalendar.isSelectionDatesChanging = false;
		rangeSelectionCalendar.UpdateDateColors();
	}

	// Day cells are reused on navigation and UpdateDayGlobalProperties resets every cell's
	// SelectedBackgroundColor, so the range colors and boundaries are re-applied after each pass.
	private protected override void OnDaysUpdated() => UpdateDateColors();

	void UpdateDateColors()
	{
		// The selection engine also drives IsSelected, so the boundaries are taken from it rather
		// than from SelectedStartDate/SelectedEndDate, which keep their last value after the
		// selection is cleared. selectionEngine is still null while the base constructor renders.
		var rangeStart = selectionEngine?.RangeSelectionStartDate;
		var rangeEnd = selectionEngine?.RangeSelectionEndDate;
		var hasRange = rangeStart.HasValue && rangeEnd.HasValue;

		foreach (var dayView in dayViews)
		{
			if (dayView.BindingContext is DayModel dayModel)
			{
				// Assigned on every cell (not only selected ones) so a reused cell never keeps
				// the boundary flags of the date it showed before.
				dayModel.IsRangeStart = hasRange && dayModel.IsSelected && dayModel.Date == rangeStart.Value.Date;
				dayModel.IsRangeEnd = hasRange && dayModel.IsSelected && dayModel.Date == rangeEnd.Value.Date;

				if (SelectedDates?.Contains(dayModel.Date) == true)
				{
					dayModel.SelectedBackgroundColor = SelectedDatesRangeBackgroundColor;
				}

				if (dayModel.Date == SelectedStartDate || dayModel.Date == SelectedEndDate)
				{
					dayModel.SelectedBackgroundColor = SelectedDayBackgroundColor;
				}
			}
		}
	}
}
