using System.Collections;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Styles;
using Plugin.Maui.Calendar.Shared.Extensions;
using System.Collections.Specialized;
using System.Collections.ObjectModel;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for Day
	/// </summary>
	public static readonly BindableProperty DayProperty = BindableProperty.Create(
		nameof(Day),
		typeof(int),
		typeof(Calendar),
		1,
		BindingMode.TwoWay,
		propertyChanged: OnDayChanged,
		defaultValueCreator: static _ => DateTime.Today.Day
	);

	/// <summary>
	/// Number signifying the day currently selected in the picker
	/// </summary>
	public int Day
	{
		get => (int)GetValue(DayProperty);
		set => SetValue(DayProperty, value);
	}

	static void OnDayChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		var newDay = (int)newValue;

		if (calendar.ShownDate.Day != newDay)
		{
			// A day the shown month doesn't have (31 in a 30-day month) moves to its last day.
			calendar.ShownDate = new DateTime(
				calendar.Year,
				calendar.Month,
				Math.Min(DateTime.DaysInMonth(calendar.Year, calendar.Month), newDay)
			);
		}
	}


	/// <summary>
	/// Bindable property for Month
	/// </summary>
	public static readonly BindableProperty MonthProperty = BindableProperty.Create(
		nameof(Month),
		typeof(int),
		typeof(Calendar),
		1,
		BindingMode.TwoWay,
		propertyChanged: OnMonthChanged,
		defaultValueCreator: static _ => DateTime.Today.Month
	);

	/// <summary>
	/// Number signifying the month currently selected in the picker
	/// </summary>
	public int Month
	{
		get => (int)GetValue(MonthProperty);
		set => SetValue(MonthProperty, value);
	}

	static void OnMonthChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (newValue is not int newMonth || newMonth <= 0 || newMonth > 12)
		{
			throw new ArgumentException("Month must be between 1 and 12.");
		}

		var calendar = (Calendar)bindable;

		if (calendar.ShownDate.Month != newMonth)
		{
			calendar.ShownDate = new DateTime(
				calendar.Year,
				newMonth,
				Math.Min(DateTime.DaysInMonth(calendar.Year, newMonth), calendar.Day)
			);
		}
	}


	/// <summary>
	/// Bindable property for YearProperty
	/// </summary>
	public static readonly BindableProperty YearProperty = BindableProperty.Create(
		nameof(Year),
		typeof(int),
		typeof(Calendar),
		1,
		BindingMode.TwoWay,
		propertyChanged: OnYearChanged,
		defaultValueCreator: static _ => DateTime.Today.Year
	);

	/// <summary>
	/// Number signifying the year currently selected in the picker
	/// </summary>
	public int Year
	{
		get => (int)GetValue(YearProperty);
		set => SetValue(YearProperty, value);
	}

	static void OnYearChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		var newYear = (int)newValue;

		if (calendar.ShownDate.Year != newYear)
		{
			// February 29 moves to February 28 in a year that is not a leap year.
			calendar.ShownDate = new DateTime(
				newYear,
				calendar.Month,
				Math.Min(DateTime.DaysInMonth(newYear, calendar.Month), calendar.Day)
			);
		}
	}


	/// <summary>
	/// Bindable property for InitalDate
	/// </summary>
	public static readonly BindableProperty ShownDateProperty = BindableProperty.Create(
		nameof(ShownDate),
		typeof(DateTime),
		typeof(Calendar),
		default(DateTime),
		BindingMode.TwoWay,
		propertyChanged: OnShownDateChanged,
		defaultValueCreator: static _ => DateTime.Today
	);

	/// <summary>
	/// Specifies the Date that is initially shown
	/// </summary>
	public DateTime ShownDate
	{
		get => (DateTime)GetValue(ShownDateProperty);
		set => SetValue(ShownDateProperty, value);
	}

	static void OnShownDateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		var newDateTime = (DateTime)newValue;

		// Day, Month and Year follow ShownDate; setting them only moves ShownDate when it differs.
		calendar.Day = newDateTime.Day;
		calendar.Month = newDateTime.Month;
		calendar.Year = newDateTime.Year;

		calendar.UpdateLayoutUnitLabel();
		calendar.UpdateDays();

		calendar.OnShownDateChangedCommand?.Execute(calendar.ShownDate);

		calendar.OnPropertyChanged(nameof(calendar.LocalizedYear));

		if (calendar.CurrentSelectionEngine is RangedSelectionEngine)
		{
			calendar.UpdateRangeSelection();
		}

		calendar.RefreshNavigationCommands();
	}


	/// <summary>
	/// Bindable property for InitalDate
	/// </summary>
	public static readonly BindableProperty OnShownDateChangedCommandProperty = BindableProperty.Create(
			nameof(OnShownDateChangedCommand),
			typeof(ICommand),
			typeof(Calendar),
			null
		);

	/// <summary>
	/// Specifies the Date that is initially shown
	/// </summary>
	public ICommand OnShownDateChangedCommand
	{
		get => (ICommand)GetValue(OnShownDateChangedCommandProperty);
		set => SetValue(OnShownDateChangedCommandProperty, value);
	}


	/// <summary>
	/// Bindable property for ShowMonthPicker
	/// </summary>
	public static readonly BindableProperty ShowMonthPickerProperty = BindableProperty.Create(
		nameof(ShowMonthPicker),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Determines whether the monthPicker should be shown
	/// </summary>
	public bool ShowMonthPicker
	{
		get => (bool)GetValue(ShowMonthPickerProperty);
		set => SetValue(ShowMonthPickerProperty, value);
	}

	/// <summary>
	/// Bindable property for ShowYearPicker
	/// </summary>
	public static readonly BindableProperty ShowYearPickerProperty = BindableProperty.Create(
		nameof(ShowYearPicker),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Determines whether the yearPicker should be shown
	/// </summary>
	public bool ShowYearPicker
	{
		get => (bool)GetValue(ShowYearPickerProperty);
		set => SetValue(ShowYearPickerProperty, value);
	}


	/// <summary>
	/// Bindable property for Culture
	/// </summary>
	public static readonly BindableProperty CultureProperty = BindableProperty.Create(
		nameof(Culture),
		typeof(CultureInfo),
		typeof(Calendar),
		CultureInfo.InvariantCulture,
		BindingMode.TwoWay,
		propertyChanged: OnCultureChanged,
		// A binding to a culture that is not loaded yet sets null.
		coerceValue: static (bindable, value) => value ?? CultureInfo.InvariantCulture
	);

	/// <summary>
	/// Specifies the culture to be used
	/// </summary>
	public CultureInfo Culture
	{
		get => (CultureInfo)GetValue(CultureProperty);
		set => SetValue(CultureProperty, value);
	}


	static void OnCultureChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.formattingCulture = null;

		calendar.UpdateLayoutUnitLabel();
		calendar.UpdateSelectedDateLabel();
		calendar.UpdateDayTitles();
		calendar.UpdateDays();
		calendar.OnPropertyChanged(nameof(calendar.LocalizedYear));
	}

	CultureInfo formattingCulture;

	/// <summary>
	/// <see cref="Culture"/> with the Gregorian calendar. The days are always laid out by the
	/// Gregorian calendar, so month names and dates must be written by it too: the default calendar
	/// of some cultures is another one (Persian for fa-IR, Um Al-Qura for ar-SA, Thai Buddhist for
	/// th-TH), whose month names and years would not match the days shown.
	/// </summary>
	internal CultureInfo FormattingCulture => formattingCulture ??= GetGregorianCulture(Culture);

	internal static CultureInfo GetGregorianCulture(CultureInfo culture)
	{
		if (culture.DateTimeFormat.Calendar is GregorianCalendar)
		{
			return culture;
		}

		// Every culture offers the localized Gregorian calendar (the default type), which keeps the
		// culture's own month names (for example "اکتبر" for October in Persian).
		var gregorianCulture = (CultureInfo)culture.Clone();
		gregorianCulture.DateTimeFormat.Calendar = new GregorianCalendar();
		return gregorianCulture;
	}

	/// <summary>
	/// Bindable property for UseNativeDigits
	/// </summary>
	public static readonly BindableProperty UseNativeDigitsProperty = BindableProperty.Create(
		nameof(UseNativeDigits),
		typeof(bool),
		typeof(Calendar),
		false,
		propertyChanged: OnUseNativeDigitsChanged
	);

	/// <summary>
	/// Determines whether digits in calendar UI should be displayed using the native digits 
	/// of the specified culture (e.g., Arabic, Hindi).
	/// If set to true, numbers will be localized according to the culture's native digits;
	/// otherwise, standard Western digits ("0"–"9") will be used.
	/// </summary>
	public bool UseNativeDigits
	{
		get => (bool)GetValue(UseNativeDigitsProperty);
		set => SetValue(UseNativeDigitsProperty, value);
	}

	static void OnUseNativeDigitsChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;

		calendar.UpdateLayoutUnitLabel();
		calendar.UpdateSelectedDateLabel();
		calendar.UpdateDays();
		calendar.OnPropertyChanged(nameof(calendar.LocalizedYear));
	}

	/// <summary>
	/// Bindable property for MonthText
	/// </summary>
	public static readonly BindableProperty MonthTextProperty = BindableProperty.Create(
		nameof(LayoutUnitText),
		typeof(string),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Culture specific text specifying the name of the month
	/// </summary>
	public string LayoutUnitText
	{
		get => (string)GetValue(MonthTextProperty);
		set => SetValue(MonthTextProperty, value);
	}


	/// <summary>
	/// Bindable property for OtherMonthDayIsVisible
	/// </summary>
	public static readonly BindableProperty OtherMonthDayIsVisibleProperty = BindableProperty.Create(
			nameof(OtherMonthDayIsVisible),
			typeof(bool),
			typeof(Calendar),
			true,
			propertyChanged: OnOtherMonthDayIsVisibleChanged
		);

	/// <summary>
	/// Specifies whether the days belonging to a month other than the selected one will be shown
	/// </summary>
	public bool OtherMonthDayIsVisible
	{
		get => (bool)GetValue(OtherMonthDayIsVisibleProperty);
		set => SetValue(OtherMonthDayIsVisibleProperty, value);
	}

	static void OnOtherMonthDayIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.UpdateDays();
	}


	/// <summary>
	/// Bindable property for OtherMonthWeekIsVisible
	/// </summary>
	public static readonly BindableProperty OtherMonthWeekIsVisibleProperty = BindableProperty.Create(
		nameof(OtherMonthWeekIsVisible),
		typeof(bool),
		typeof(Calendar),
		true,
		propertyChanged: OnOtherMonthWeekIsVisibleChanged
	);

	/// <summary>
	/// Specifies whether the weeks belonging to a month other than the selected one will be shown
	/// </summary>
	public bool OtherMonthWeekIsVisible
	{
		get => (bool)GetValue(OtherMonthWeekIsVisibleProperty);
		set => SetValue(OtherMonthWeekIsVisibleProperty, value);
	}

	static void OnOtherMonthWeekIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;

		calendar.UpdateDays();
	}


	/// <summary>
	/// Binding property for CalendarSectionShown
	/// </summary>
	public static readonly BindableProperty CalendarSectionShownProperty = BindableProperty.Create(
		nameof(CalendarSectionShown),
		typeof(bool),
		typeof(Calendar),
		true,
		propertyChanged: OnCalendarSectionShownChanged
	);

	/// <summary>
	/// Specifies whether the calendar section is shown
	/// </summary>
	public bool CalendarSectionShown
	{
		get => (bool)GetValue(CalendarSectionShownProperty);
		set => SetValue(CalendarSectionShownProperty, value);
	}

	static void OnCalendarSectionShownChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.ShowHideCalendarSection();
	}



	/// <summary>
	/// Bindable property for DayTapped
	/// </summary>
	public static readonly BindableProperty DayTappedCommandProperty = BindableProperty.Create(
		nameof(DayTappedCommand),
		typeof(ICommand),
		typeof(Calendar),
		null,
		propertyChanged: OnDayTappedCommandChanged
	);

	/// <summary>
	/// Action to run after a day has been tapped.
	/// </summary>
	public ICommand DayTappedCommand
	{
		get => (ICommand)GetValue(DayTappedCommandProperty);
		set => SetValue(DayTappedCommandProperty, value);
	}

	static void OnDayTappedCommandChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.UpdateDayGlobalProperties();
	}


	/// <summary> Bindable property for MinimumDate </summary>
	public static readonly BindableProperty MinimumDateProperty = BindableProperty.Create(
		nameof(MinimumDate),
		typeof(DateTime),
		typeof(Calendar),
		DateTime.MinValue,
		propertyChanged: OnMinMaxDateChanged
	);

	/// <summary> Minimum date which can be selected </summary>
	public DateTime MinimumDate
	{
		get => (DateTime)GetValue(MinimumDateProperty);
		set => SetValue(MinimumDateProperty, value);
	}

	/// <summary> Bindable property for MaximumDate </summary>
	public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(
		nameof(MaximumDate),
		typeof(DateTime),
		typeof(Calendar),
		DateTime.MaxValue,
		propertyChanged: OnMinMaxDateChanged
	);

	/// <summary> Maximum date which can be selected </summary>
	public DateTime MaximumDate
	{
		get => (DateTime)GetValue(MaximumDateProperty);
		set => SetValue(MaximumDateProperty, value);
	}

	static void OnMinMaxDateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;

		calendar.UpdateDays();
		calendar.RefreshNavigationCommands();
	}


	/// <summary>
	/// Bindable property for WeekLayout
	/// </summary>
	public static readonly BindableProperty CalendarLayoutProperty = BindableProperty.Create(
		nameof(CalendarLayout),
		typeof(WeekLayout),
		typeof(Calendar),
		WeekLayout.Month,
		propertyChanged: OnCalendarLayoutChanged
	);

	/// <summary>
	/// Sets the layout of the calendar
	/// </summary>
	public WeekLayout CalendarLayout
	{
		get => (WeekLayout)GetValue(CalendarLayoutProperty);
		set => SetValue(CalendarLayoutProperty, value);
	}

	// RenderLayout already runs a forced UpdateDays for the new cells.
	static void OnCalendarLayoutChanged(BindableObject bindable, object oldValue, object newValue) =>
		((Calendar)bindable).RenderLayout();


	/// <summary>
	/// Bindable property for WeekViewUnit
	/// </summary>
	public static readonly BindableProperty WeekViewUnitProperty = BindableProperty.Create(
		nameof(WeekViewUnit),
		typeof(WeekViewUnit),
		typeof(Calendar),
		WeekViewUnit.MonthName,
		propertyChanged: OnWeekViewUnitChanged
	);

	/// <summary>
	/// Sets the display name of the calendar unit
	/// </summary>
	public WeekViewUnit WeekViewUnit
	{
		get => (WeekViewUnit)GetValue(WeekViewUnitProperty);
		set => SetValue(WeekViewUnitProperty, value);
	}

	static void OnWeekViewUnitChanged(BindableObject bindable, object oldValue, object newValue) =>
		((Calendar)bindable).UpdateLayoutUnitLabel();


	/// <summary>
	/// Bindable property for FirstDayOfWeek
	/// </summary>
	public static readonly BindableProperty FirstDayOfWeekProperty = BindableProperty.Create(
		nameof(FirstDayOfWeek),
		typeof(DayOfWeek),
		typeof(Calendar),
		DayOfWeek.Sunday,
		propertyChanged: OnFirstDayOfWeekChanged
	);

	/// <summary>
	/// Sets the first day of the week in the calendar
	/// </summary>
	public DayOfWeek FirstDayOfWeek
	{
		get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
		set => SetValue(FirstDayOfWeekProperty, value);
	}

	static void OnFirstDayOfWeekChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;

		calendar.UpdateLayoutUnitLabel();
		calendar.UpdateSelectedDateLabel();
		calendar.RenderLayout();
	}


	/// <summary>
	/// Bindable property for MonthChangedCommand
	/// </summary>
	public static readonly BindableProperty MonthChangedCommandProperty = BindableProperty.Create(
		nameof(MonthChangedCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Command that is executed when the month changes.
	/// </summary>
	public ICommand MonthChangedCommand
	{
		get => (ICommand)GetValue(MonthChangedCommandProperty);
		set => SetValue(MonthChangedCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for ShownDatesChangedCommand.
	/// </summary>
	public static readonly BindableProperty ShownDatesChangedCommandProperty = BindableProperty.Create(
		nameof(ShownDatesChangedCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Command that is executed when the visible date range changes.
	/// </summary>
	public ICommand ShownDatesChangedCommand
	{
		get => (ICommand)GetValue(ShownDatesChangedCommandProperty);
		set => SetValue(ShownDatesChangedCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for AllowDeselect
	/// </summary>
	public static readonly BindableProperty AllowDeselectingProperty = BindableProperty.Create(
		nameof(AllowDeselecting),
		typeof(bool),
		typeof(Calendar),
		true,
		propertyChanged: OnAllowDeselectingChanged
	);

	/// <summary>
	/// Indicates whether the date selection can be deselected
	/// </summary>
	public bool AllowDeselecting
	{
		get => (bool)GetValue(AllowDeselectingProperty);
		set => SetValue(AllowDeselectingProperty, value);
	}

	static void OnAllowDeselectingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.UpdateDayGlobalProperties();
	}
}
