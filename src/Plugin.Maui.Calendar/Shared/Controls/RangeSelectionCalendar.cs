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
		Colors.Transparent
	);

	/// <summary>
	/// Background color for the range between SelectedStartDate and SelectedEndDate.
	/// </summary>
	public Color SelectedDatesRangeBackgroundColor
	{
		get
		{
			var color = (Color)GetValue(SelectedDatesRangeBackgroundColorProperty);

			if (color == Colors.Transparent)
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
			SetValue(SelectedStartDateProperty, first.First());
			SetValue(SelectedEndDateProperty, first.Last());
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
			rangeSelectionCalendar.SelectedDates = rangeSelectionCalendar.selectionEngine.GetDateRange(rangeSelectionCalendar.DisabledDates);
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
			rangeSelectionCalendar.SelectedDates = rangeSelectionCalendar.selectionEngine.GetDateRange();
		}
		rangeSelectionCalendar.isSelectionDatesChanging = false;

		rangeSelectionCalendar.UpdateDateColors();
	}

	void UpdateDateColors()
	{
		foreach (var dayView in dayViews)
		{
			var dayModel = dayView.BindingContext as DayModel;

			if (SelectedDates.Contains(dayModel.Date))
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
