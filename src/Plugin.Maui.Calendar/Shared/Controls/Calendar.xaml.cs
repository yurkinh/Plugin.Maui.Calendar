using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	#region Private fields
	SwipeGestureRecognizer leftSwipeGesture;
	SwipeGestureRecognizer rightSwipeGesture;
	SwipeGestureRecognizer upSwipeGesture;
	SwipeGestureRecognizer downSwipeGesture;

	const uint calendarSectionAnimationRate = 16;
	const int calendarSectionAnimationDuration = 200;
	const string calendarSectionAnimationId = nameof(calendarSectionAnimationId);
	readonly Lazy<Animation> calendarSectionAnimateHide;
	readonly Lazy<Animation> calendarSectionAnimateShow;
	bool calendarSectionAnimating;
	double calendarSectionHeight;
	IViewLayoutEngine CurrentViewLayoutEngine { get; set; }
	public ISelectionEngine CurrentSelectionEngine { get; set; } = new SingleSelectionEngine();
	readonly Dictionary<string, bool> propertyChangedNotificationSupressions = [];
	protected readonly List<DayView> dayViews = [];

	#endregion

	#region Events

	/// <summary>
	/// Event that is triggered when the month changes.
	/// </summary>
	public event EventHandler<MonthChangedEventArgs> MonthChanged;
	public event EventHandler SwipedLeft;
	public event EventHandler SwipedRight;
	public event EventHandler SwipedUp;
	public event EventHandler SwipedDown;

	#endregion

	#region Ctor
	/// <summary>
	/// Calendar plugin for .NET MAUI
	/// </summary>
	public Calendar()
	{
		PrevLayoutUnitCommand = new Command(PrevUnit);
		NextLayoutUnitCommand = new Command(NextUnit);
		PrevYearCommand = new Command(PrevYear);
		NextYearCommand = new Command(NextYear);
		ShowHideCalendarCommand = new Command(ToggleCalendarSectionVisibility);

		InitializeComponent();

		InitializeViewLayoutEngine();
		InitializeSelectionType();
		UpdateSelectedDateLabel();
		UpdateLayoutUnitLabel();
		UpdateEvents();
		RenderLayout();

		calendarSectionAnimateHide = new Lazy<Animation>(() => new Animation(AnimateMonths, 1, 0));
		calendarSectionAnimateShow = new Lazy<Animation>(() => new Animation(AnimateMonths, 0, 1));
	}
	#endregion

	protected override void OnHandlerChanged()
	{
		//subscribe
		base.OnHandlerChanged();
		calendarContainer.SizeChanged += OnCalendarContainerSizeChanged;
		if (!SwipeDetectionDisabled)
		{
			leftSwipeGesture = new() { Direction = SwipeDirection.Left };
			rightSwipeGesture = new() { Direction = SwipeDirection.Right };
			upSwipeGesture = new() { Direction = SwipeDirection.Up };
			downSwipeGesture = new() { Direction = SwipeDirection.Down };

			leftSwipeGesture.Swiped += OnSwiped;
			rightSwipeGesture.Swiped += OnSwiped;
			upSwipeGesture.Swiped += OnSwiped;
			downSwipeGesture.Swiped += OnSwiped;

			GestureRecognizers.Add(leftSwipeGesture);
			GestureRecognizers.Add(rightSwipeGesture);
			GestureRecognizers.Add(upSwipeGesture);
			GestureRecognizers.Add(downSwipeGesture);
		}
	}

	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		//unsunscribe
		base.OnHandlerChanging(args);

		if (args.OldHandler != null)
		{
			calendarContainer.SizeChanged -= OnCalendarContainerSizeChanged;

			if (!SwipeDetectionDisabled && GestureRecognizers.Count > 0)
			{
				leftSwipeGesture.Swiped -= OnSwiped;
				rightSwipeGesture.Swiped -= OnSwiped;
				upSwipeGesture.Swiped -= OnSwiped;
				downSwipeGesture.Swiped -= OnSwiped;

				GestureRecognizers.Remove(leftSwipeGesture);
				GestureRecognizers.Remove(rightSwipeGesture);
				GestureRecognizers.Remove(upSwipeGesture);
				GestureRecognizers.Remove(downSwipeGesture);
			}
			//Todo remove later/when all event and properties will be refactored
			//all this should be done automaticall or not needed
			Dispose();
		}
	}


	#region Bindable properties

	/// <summary>
	/// Bindable property for Day
	/// </summary>
	public static readonly BindableProperty DayProperty = BindableProperty.Create(
		nameof(Day),
		typeof(int),
		typeof(Calendar),
		DateTime.Today.Day,
		BindingMode.TwoWay,
		propertyChanged: OnDayChanged
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
		if (bindable is Calendar calendar && newValue is int newDay && calendar.ShownDate.Day != newDay)
		{
			calendar.ShownDate = new DateTime(calendar.Year, calendar.Month, newDay);
		}
	}


	/// <summary>
	/// Bindable property for Month
	/// </summary>
	public static readonly BindableProperty MonthProperty = BindableProperty.Create(
		nameof(Month),
		typeof(int),
		typeof(Calendar),
		DateTime.Today.Month,
		BindingMode.TwoWay,
		propertyChanged: OnMonthChanged
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

		if (bindable is Calendar calendar && calendar.ShownDate.Month != newMonth)
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
		DateTime.Today.Year,
		BindingMode.TwoWay,
		propertyChanged: OnYearChanged
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
		if (bindable is Calendar calendar && calendar.ShownDate.Year != (int)newValue)
		{
			calendar.ShownDate = new DateTime((int)newValue, calendar.Month, calendar.Day);
		}
	}


	/// <summary>
	/// Bindable property for InitalDate
	/// </summary>
	public static readonly BindableProperty ShownDateProperty = BindableProperty.Create(
		nameof(ShownDate),
		typeof(DateTime),
		typeof(Calendar),
		DateTime.Today,
		BindingMode.TwoWay,
		propertyChanged: OnShownDateChanged
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
		if (bindable is Calendar calendar && newValue is DateTime newDateTime)
		{
			if (calendar.Day != newDateTime.Day)
			{
				calendar.Day = newDateTime.Day;
			}

			if (calendar.Month != newDateTime.Month)
			{
				calendar.Month = newDateTime.Month;
			}

			if (calendar.Year != newDateTime.Year)
			{
				calendar.Year = newDateTime.Year;
			}

			calendar.UpdateLayoutUnitLabel();
			calendar.UpdateDays();

			calendar.OnShownDateChangedCommand?.Execute(calendar.ShownDate);
		}
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
		propertyChanged: OnCultureChanged
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
		if (bindable is Calendar calendar)
		{
			if (calendar.ShownDate.Month > 0)
			{
				calendar.UpdateLayoutUnitLabel();
			}

			calendar.UpdateSelectedDateLabel();
		}
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
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDays();
		}
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
		if (bindable is Calendar calendar)
		{
			calendar.ShowHideCalendarSection();
		}
	}



	/// <summary>
	/// Bindable property for DayTapped
	/// </summary>
	public static readonly BindableProperty DayTappedCommandProperty = BindableProperty.Create(
		nameof(DayTappedCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Action to run after a day has been tapped.
	/// </summary>
	public ICommand DayTappedCommand
	{
		get => (ICommand)GetValue(DayTappedCommandProperty);
		set => SetValue(DayTappedCommandProperty, value);
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
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDays(); ;
		}
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

	static void OnCalendarLayoutChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar && newValue is WeekLayout layout)
		{
			calendar.CalendarLayout = layout;

			calendar.CurrentViewLayoutEngine = layout switch
			{
				WeekLayout.Week => new WeekViewEngine(1, calendar.FirstDayOfWeek),
				WeekLayout.TwoWeek => new WeekViewEngine(2, calendar.FirstDayOfWeek),
				_ => new MonthViewEngine(calendar.FirstDayOfWeek),
			};

			calendar.RenderLayout();
			calendar.UpdateDays();
		}
	}


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

	static void OnWeekViewUnitChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar && newValue is WeekViewUnit viewUnit)
		{
			calendar.WeekViewUnit = viewUnit;
		}
	}


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
		if (bindable is Calendar calendar)
		{
			if (calendar.ShownDate.Month > 0)
			{
				calendar.UpdateLayoutUnitLabel();
			}

			calendar.UpdateSelectedDateLabel();
		}
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
	/// Bindable property for AllowDeselect
	/// </summary>
	public static readonly BindableProperty AllowDeselectingProperty = BindableProperty.Create(
		nameof(AllowDeselecting),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Indicates whether the date selection can be deselected
	/// </summary>
	public bool AllowDeselecting
	{
		get => (bool)GetValue(AllowDeselectingProperty);
		set => SetValue(AllowDeselectingProperty, value);
	}

	#endregion

	#region Color BindableProperties
	/// <summary>
	/// Bindable property for MonthLabelColor
	/// </summary>
	public static readonly BindableProperty MonthLabelColorProperty = BindableProperty.Create(
		nameof(MonthLabelColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#2196F3")
	);

	/// <summary>
	/// Specifies the color of the month label
	/// </summary>
	public Color MonthLabelColor
	{
		get => (Color)GetValue(MonthLabelColorProperty);
		set => SetValue(MonthLabelColorProperty, value);
	}

	/// <summary>
	/// Bindable property for YearLabelColor
	/// </summary>
	public static readonly BindableProperty YearLabelColorProperty = BindableProperty.Create(
		nameof(YearLabelColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#2196F3")
	);

	/// <summary>
	/// Specifies the color of the year label
	/// </summary>
	public Color YearLabelColor
	{
		get => (Color)GetValue(YearLabelColorProperty);
		set => SetValue(YearLabelColorProperty, value);
	}


	/// <summary>
	/// Bindable property for WeekendDayColor
	/// </summary>
	public static readonly BindableProperty WeekendDayColorProperty = BindableProperty.Create(
		nameof(WeekendDayColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black,
		propertyChanged: OnWeekendDayColorChanged
	);

	/// <summary>
	/// Specifies the color of days belonging to a month other than the selected one
	/// </summary>
	public Color WeekendDayColor
	{
		get => (Color)GetValue(WeekendDayColorProperty);
		set => SetValue(WeekendDayColorProperty, value);
	}

	static void OnWeekendDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for OtherMonthDayColor
	/// </summary>
	public static readonly BindableProperty OtherMonthDayColorProperty = BindableProperty.Create(
		nameof(OtherMonthDayColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Silver,
		propertyChanged: OnOtherMonthDayColorChanged
	);

	/// <summary>
	/// Specifies the color of days belonging to a month other than the selected one
	/// </summary>
	public Color OtherMonthDayColor
	{
		get => (Color)GetValue(OtherMonthDayColorProperty);
		set => SetValue(OtherMonthDayColorProperty, value);
	}

	static void OnOtherMonthDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}

	#endregion


	#region DayView BindableProperties
	/// <summary>
	/// Bindable property for DayViewSize
	/// </summary>
	public static readonly BindableProperty DayViewSizeProperty = BindableProperty.Create(
		nameof(DayViewSize),
		typeof(double),
		typeof(Calendar),
		40.0
	);

	/// <summary>
	/// Specifies the size of individual dates
	/// </summary>
	public double DayViewSize
	{
		get => (double)GetValue(DayViewSizeProperty);
		set => SetValue(DayViewSizeProperty, value);
	}


	/// <summary>
	/// Bindable property for DayViewFontSizeProperty
	/// </summary>
	public static readonly BindableProperty DayViewFontSizeProperty = BindableProperty.Create(
		nameof(DayViewFontSize),
		typeof(double),
		typeof(Calendar),
		14d,
		propertyChanged: OnDayViewFontSizeChanged
	);

	/// <summary>
	/// Specifies the FontSize of DayView label
	/// </summary>
	[TypeConverter(typeof(FontSizeConverter))]
	public double DayViewFontSize
	{
		get => (double)GetValue(DayViewFontSizeProperty);
		set => SetValue(DayViewFontSizeProperty, value);
	}

	static void OnDayViewFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for DayViewCornerRadius
	/// </summary>
	public static readonly BindableProperty DayViewCornerRadiusProperty = BindableProperty.Create(
		nameof(DayViewCornerRadius),
		typeof(float),
		typeof(Calendar),
		20f,
		propertyChanged: OnDayViewCornerRadiusChanged
	);

	/// <summary>
	/// Specifies the corner radius of individual dates
	/// </summary>
	public float DayViewCornerRadius
	{
		get => (float)GetValue(DayViewCornerRadiusProperty);
		set => SetValue(DayViewCornerRadiusProperty, value);
	}

	static void OnDayViewCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDays();
		}
	}


	/// <summary>
	/// Bindable property for DaysLabelStyle
	/// </summary>
	public static readonly BindableProperty DaysLabelStyleProperty = BindableProperty.Create(
		nameof(DaysLabelStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultLabelStyle
	);

	/// <summary>
	/// Specifies the style of day labels
	/// </summary>
	public Style DaysLabelStyle
	{
		get => (Style)GetValue(DaysLabelStyleProperty);
		set => SetValue(DaysLabelStyleProperty, value);
	}

	#endregion

	#region DisabledDates BindableProperty
	public static readonly BindableProperty DisabledDatesProperty = BindableProperty.Create(
		  nameof(DisabledDates),
		  typeof(List<DateTime>),
		  typeof(Calendar),
		  defaultValue: new List<DateTime>(),
		  BindingMode.TwoWay
	  );

	public List<DateTime> DisabledDates
	{
		get => (List<DateTime>)GetValue(DisabledDatesProperty);
		set => SetValue(DisabledDatesProperty, value);
	}

	/// <summary> Bindable property for DisabledDayColor </summary>
	public static readonly BindableProperty DisabledDayColorProperty = BindableProperty.Create(
		nameof(DisabledDayColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#ECECEC"),
		propertyChanged: OnDisabledDayColorChanged
	);

	/// <summary> Color for days which are out of MinimumDate - MaximumDate range </summary>
	public Color DisabledDayColor
	{
		get => (Color)GetValue(DisabledDayColorProperty);
		set => SetValue(DisabledDayColorProperty, value);
	}

	static void OnDisabledDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}

	#endregion

	#region Events BindableProperties

	/// <summary>
	/// Bindable property for EventIndicatorType
	/// </summary>
	public static readonly BindableProperty EventIndicatorTypeProperty = BindableProperty.Create(
		nameof(EventIndicatorType),
		typeof(EventIndicatorType),
		typeof(Calendar),
		EventIndicatorType.BottomDot,
		propertyChanged: OnEventIndicatorTypeChanged
	);

	/// <summary>
	/// Specifies the way in which events will be shown on dates
	/// </summary>
	public EventIndicatorType EventIndicatorType
	{
		get => (EventIndicatorType)GetValue(EventIndicatorTypeProperty);
		set => SetValue(EventIndicatorTypeProperty, value);
	}

	static void OnEventIndicatorTypeChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorColor
	/// </summary>
	public static readonly BindableProperty EventIndicatorColorProperty = BindableProperty.Create(
		nameof(EventIndicatorColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#FF4081"),
		propertyChanged: OnEventIndicatorColorChanged
	);

	/// <summary>
	/// Specifies the color of the event indicators
	/// </summary>
	public Color EventIndicatorColor
	{
		get => (Color)GetValue(EventIndicatorColorProperty);
		set => SetValue(EventIndicatorColorProperty, value);
	}

	static void OnEventIndicatorColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorSelectedColor
	/// </summary>
	public static readonly BindableProperty EventIndicatorSelectedColorProperty = BindableProperty.Create(
			nameof(EventIndicatorSelectedColor),
			typeof(Color),
			typeof(Calendar),
			Color.FromArgb("#FF4081"),
			propertyChanged: OnEventIndicatorSelectedColorChanged
		);

	/// <summary>
	/// Specifies the color of the event indicators on selected dates
	/// </summary>
	public Color EventIndicatorSelectedColor
	{
		get => (Color)GetValue(EventIndicatorSelectedColorProperty);
		set => SetValue(EventIndicatorSelectedColorProperty, value);
	}

	static void OnEventIndicatorSelectedColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorTextColor
	/// </summary>
	public static readonly BindableProperty EventIndicatorTextColorProperty = BindableProperty.Create(
			nameof(EventIndicatorTextColor),
			typeof(Color),
			typeof(Calendar),
			Colors.Black,
			propertyChanged: OnEventIndicatorTextColorChanged
		);

	/// <summary>
	/// Specifies the color of the event indicator text
	/// </summary>
	public Color EventIndicatorTextColor
	{
		get => (Color)GetValue(EventIndicatorTextColorProperty);
		set => SetValue(EventIndicatorTextColorProperty, value);
	}

	static void OnEventIndicatorTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorSelectedText
	/// </summary>
	public static readonly BindableProperty EventIndicatorSelectedTextColorProperty = BindableProperty.Create(
			nameof(EventIndicatorSelectedTextColor),
			typeof(Color),
			typeof(Calendar),
			Colors.Black,
			propertyChanged: OnEventIndicatorSelectedTextColorChanged
		);

	/// <summary>
	/// Specifies the color of the event indicator text on selected dates
	/// </summary>
	public Color EventIndicatorSelectedTextColor
	{
		get => (Color)GetValue(EventIndicatorSelectedTextColorProperty);
		set => SetValue(EventIndicatorSelectedTextColorProperty, value);
	}

	static void OnEventIndicatorSelectedTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for EventsScrollView
	/// </summary>
	public static readonly BindableProperty EventsScrollViewVisibleProperty = BindableProperty.Create(
			nameof(EventsScrollViewVisible),
			typeof(bool),
			typeof(Calendar),
			false
		);

	/// <summary>
	/// Specifies whether the events section is visible
	/// </summary>
	public bool EventsScrollViewVisible
	{
		get => (bool)GetValue(EventsScrollViewVisibleProperty);
		set => SetValue(EventsScrollViewVisibleProperty, value);
	}


	/// <summary>
	/// Bindable property for events
	/// </summary>
	public static readonly BindableProperty EventsProperty = BindableProperty.Create(
		nameof(Events),
		typeof(EventCollection),
		typeof(Calendar),
		new EventCollection(),
		propertyChanged: OnEventsChanged
	);

	/// <summary>
	/// Collection of all the events in the calendar
	/// </summary>
	public EventCollection Events
	{
		get => (EventCollection)GetValue(EventsProperty);
		set => SetValue(EventsProperty, value);
	}

	static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			if (oldValue is EventCollection oldEvents)
			{
				oldEvents.CollectionChanged -= calendar.OnEventsCollectionChanged;
			}

			if (newValue is EventCollection newEvents)
			{
				newEvents.CollectionChanged += calendar.OnEventsCollectionChanged;
			}

			calendar.UpdateEvents();
			calendar.UpdateLayoutUnitLabel();
			//Todo: called two time at the calendar start
			calendar.UpdateDays();
		}
	}


	/// <summary>
	/// Bindable property for SelectedDayEvents
	/// </summary>
	public static readonly BindableProperty SelectedDayEventsProperty = BindableProperty.Create(
		nameof(SelectedDayEvents),
		typeof(ICollection),
		typeof(Calendar),
		new List<object>()
	);

	/// <summary>
	/// Collection of events on the selected date(s)
	/// </summary>
	public ICollection SelectedDayEvents
	{
		get => (ICollection)GetValue(SelectedDayEventsProperty);
		set => SetValue(SelectedDayEventsProperty, value);
	}

	/// <summary>
	/// Bindable property for EventTemplate
	/// </summary>
	public static readonly BindableProperty EventTemplateProperty = BindableProperty.Create(
		nameof(EventTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Specifies the template to be used for showing events
	/// </summary>
	public DataTemplate EventTemplate
	{
		get => (DataTemplate)GetValue(EventTemplateProperty);
		set => SetValue(EventTemplateProperty, value);
	}

	/// <summary>
	/// Bindable property for EmptyTemplate
	/// </summary>
	public static readonly BindableProperty EmptyTemplateProperty = BindableProperty.Create(
		nameof(EmptyTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Specifies the data template to be shown when there are no events
	/// </summary>
	public DataTemplate EmptyTemplate
	{
		get => (DataTemplate)GetValue(EmptyTemplateProperty);
		set => SetValue(EmptyTemplateProperty, value);
	}
	#endregion

	#region Arrows BindableProperties
	/// <summary>
	/// Bindable property for ArrowsColor
	/// </summary>
	public static readonly BindableProperty ArrowsColorProperty = BindableProperty.Create(
		nameof(ArrowsColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black
	);

	/// <summary>
	/// Specifies the color of month and year selection arrows
	/// </summary>
	public Color ArrowsColor
	{
		get => (Color)GetValue(ArrowsColorProperty);
		set => SetValue(ArrowsColorProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsFontSizeProperty
	/// </summary>
	public static readonly BindableProperty ArrowsFontSizeProperty = BindableProperty.Create(
		nameof(ArrowsFontSize),
		typeof(double),
		typeof(Calendar),
		14d
	);

	/// <summary>
	/// Specifies the FontSize of month and year selection arrows
	/// </summary>
	[TypeConverter(typeof(FontSizeConverter))]
	public double ArrowsFontSize
	{
		get => (double)GetValue(ArrowsFontSizeProperty);
		set => SetValue(ArrowsFontSizeProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsFontFamily
	/// </summary>
	public static readonly BindableProperty ArrowsFontFamilyProperty = BindableProperty.Create(
		nameof(ArrowsFontFamily),
		typeof(string),
		typeof(Calendar),
		"OpenSansSemibold"
	);

	/// <summary>
	/// Specifies symbol for arrow prev
	/// </summary>
	public string ArrowsFontFamily
	{
		get => (string)GetValue(ArrowsFontFamilyProperty);
		set => SetValue(ArrowsFontFamilyProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsSymbolPrev
	/// </summary>
	public static readonly BindableProperty ArrowsSymbolPrevProperty = BindableProperty.Create(
		nameof(ArrowsSymbolPrev),
		typeof(string),
		typeof(Calendar),
		"←"
	);

	/// <summary>
	/// Specifies symbol for arrow prev
	/// </summary>
	public string ArrowsSymbolPrev
	{
		get => (string)GetValue(ArrowsSymbolPrevProperty);
		set => SetValue(ArrowsSymbolPrevProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsSymbolNext
	/// </summary>
	public static readonly BindableProperty ArrowsSymbolNextProperty = BindableProperty.Create(
		nameof(ArrowsSymbolNext),
		typeof(string),
		typeof(Calendar),
		"→"
	);

	/// <summary>
	/// Specifies symbol for arrow next
	/// </summary>
	public string ArrowsSymbolNext
	{
		get => (string)GetValue(ArrowsSymbolNextProperty);
		set => SetValue(ArrowsSymbolNextProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsFontAttribute
	/// </summary>
	public static readonly BindableProperty ArrowsFontAttributeProperty = BindableProperty.Create(
		nameof(ArrowsFontAttribute),
		typeof(FontAttributes),
		typeof(Calendar),
		FontAttributes.Bold
	);

	/// <summary>
	/// Specifies font attribute of the arrow
	/// </summary>
	public FontAttributes ArrowsFontAttribute
	{
		get => (FontAttributes)GetValue(ArrowsFontAttributeProperty);
		set => SetValue(ArrowsFontAttributeProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsBorderColor
	/// </summary>
	public static readonly BindableProperty ArrowsBorderColorProperty = BindableProperty.Create(
		nameof(ArrowsBorderColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black
	);

	/// <summary>
	/// Specifies the color of arrows border
	/// </summary>
	public Color ArrowsBorderColor
	{
		get => (Color)GetValue(ArrowsBorderColorProperty);
		set => SetValue(ArrowsBorderColorProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsBackgroundColor
	/// </summary>
	public static readonly BindableProperty ArrowsBackgroundColorProperty = BindableProperty.Create(
		nameof(ArrowsBackgroundColor),
		typeof(Color),
		typeof(Calendar),
		Colors.White
	);

	/// <summary>
	/// Specifies the color of arrows Background
	/// </summary>
	public Color ArrowsBackgroundColor
	{
		get => (Color)GetValue(ArrowsBackgroundColorProperty);
		set => SetValue(ArrowsBackgroundColorProperty, value);
	}

	/// <summary>
	/// Bindable property for ArrowsBorderWidth
	/// </summary>
	public static readonly BindableProperty ArrowsBorderWidthProperty = BindableProperty.Create(
		nameof(ArrowsBorderWidth),
		typeof(double),
		typeof(Calendar),
		1d
	);

	/// <summary>
	/// Specifies the ArrowsBorderWidth of month and year selection arrows
	/// </summary>

	public double ArrowsBorderWidth
	{
		get => (double)GetValue(ArrowsBorderWidthProperty);
		set => SetValue(ArrowsBorderWidthProperty, value);
	}

	#region Footer & Header BindableProperties

	/// <summary>
	/// Bindable property for FooterArrowVisible
	/// </summary>
	public static readonly BindableProperty FooterArrowVisibleProperty = BindableProperty.Create(
		nameof(FooterArrowVisible),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Specifies whether the footer expanding arrow is visible
	/// </summary>
	public bool FooterArrowVisible
	{
		get => (bool)GetValue(FooterArrowVisibleProperty);
		set => SetValue(FooterArrowVisibleProperty, value);
	}
	#endregion


	/// <summary>
	/// Bindable property for HeaderSectionVisible
	/// </summary>
	public static readonly BindableProperty HeaderSectionVisibleProperty = BindableProperty.Create(
		nameof(HeaderSectionVisible),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Specifies whether the header section is visible
	/// </summary>
	public bool HeaderSectionVisible
	{
		get => (bool)GetValue(HeaderSectionVisibleProperty);
		set => SetValue(HeaderSectionVisibleProperty, value);
	}


	/// <summary>
	/// Bindable property for FooterSectionVisible
	/// </summary>
	public static readonly BindableProperty FooterSectionVisibleProperty = BindableProperty.Create(
		nameof(FooterSectionVisible),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Specifies whether the footer section is visible
	/// </summary>
	public bool FooterSectionVisible
	{
		get => (bool)GetValue(FooterSectionVisibleProperty);
		set => SetValue(FooterSectionVisibleProperty, value);
	}

	readonly Lazy<DataTemplate> headerSectionTemplate = new(() => new DataTemplate(() => new DefaultHeaderSection()));
	/// <summary>
	/// Bindable property for HeaderSectionTemplate
	/// </summary>
	public static readonly BindableProperty HeaderSectionTemplateProperty = BindableProperty.Create(
		nameof(HeaderSectionTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		 defaultValueCreator: bindable => ((Calendar)bindable).headerSectionTemplate.Value
	);

	/// <summary>
	/// Data template for the header section
	/// </summary>
	public DataTemplate HeaderSectionTemplate
	{
		get => (DataTemplate)GetValue(HeaderSectionTemplateProperty);
		set => SetValue(HeaderSectionTemplateProperty, value);
	}

	readonly Lazy<DataTemplate> footerSectionTemplate = new(() => new DataTemplate(() => new DefaultFooterSection()));
	/// <summary>
	/// Bindable property for FooterSectionTemplate
	/// </summary>
	public static readonly BindableProperty FooterSectionTemplateProperty = BindableProperty.Create(
		nameof(FooterSectionTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		defaultValueCreator: bindable => ((Calendar)bindable).footerSectionTemplate.Value
	);

	/// <summary>
	/// Data template for the footer section
	/// </summary>
	public DataTemplate FooterSectionTemplate
	{
		get => (DataTemplate)GetValue(FooterSectionTemplateProperty);
		set => SetValue(FooterSectionTemplateProperty, value);
	}

	#endregion



	#region Today BindableProperties
	/// <summary>
	/// Bindable property for TodayOutlineColor
	/// </summary>
	public static readonly BindableProperty TodayOutlineColorProperty = BindableProperty.Create(
		nameof(TodayOutlineColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#FF4081"),
		propertyChanged: OnTodayOutlineColorChanged
	);

	/// <summary>
	/// Specifies the color of outline for today's date
	/// </summary>
	public Color TodayOutlineColor
	{
		get => (Color)GetValue(TodayOutlineColorProperty);
		set => SetValue(TodayOutlineColorProperty, value);
	}

	static void OnTodayOutlineColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for
	/// </summary>
	public static readonly BindableProperty TodayTextColorProperty = BindableProperty.Create(
		nameof(TodayTextColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black,
		propertyChanged: OnTodayTextColorChanged
	);

	/// <summary>
	/// Specifies the color of text for today's date
	/// </summary>
	public Color TodayTextColor
	{
		get => (Color)GetValue(TodayTextColorProperty);
		set => SetValue(TodayTextColorProperty, value);
	}

	static void OnTodayTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for TodayFillColor
	/// </summary>
	public static readonly BindableProperty TodayFillColorProperty = BindableProperty.Create(
		nameof(TodayFillColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Transparent,
		propertyChanged: OnTodayFillColorChanged
	);

	/// <summary>
	/// Specifies the fill for today's date
	/// </summary>
	public Color TodayFillColor
	{
		get => (Color)GetValue(TodayFillColorProperty);
		set => SetValue(TodayFillColorProperty, value);
	}

	static void OnTodayFillColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}
	#endregion


	#region DaysTitle Bindable properties

	/// <summary>
	/// Bindable property for DaysTitleLabelStyle
	/// </summary>
	public static readonly BindableProperty DaysTitleLabelStyleProperty = BindableProperty.Create(
		nameof(DaysTitleLabelStyle),
		typeof(Style),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Specifies the style of day title labels
	/// </summary>
	public Style DaysTitleLabelStyle
	{
		get => (Style)GetValue(DaysTitleLabelStyleProperty);
		set => SetValue(DaysTitleLabelStyleProperty, value);
	}


	/// <summary>
	/// Bindable property for DaysTitleColor
	/// </summary>
	public static readonly BindableProperty DaysTitleColorProperty = BindableProperty.Create(
		nameof(DaysTitleColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black,
		propertyChanged: OnDaysTitleColorChanged
	);

	/// <summary>
	/// Specifies the color for the titles of days
	/// </summary>
	public Color DaysTitleColor
	{
		get => (Color)GetValue(DaysTitleColorProperty);
		set => SetValue(DaysTitleColorProperty, value);
	}

	static void OnDaysTitleColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}


	/// <summary>
	/// Bindable property for DaysTitleMaximumLength
	/// </summary>
	public static readonly BindableProperty DaysTitleMaximumLengthProperty = BindableProperty.Create(
			nameof(DaysTitleMaximumLength),
			typeof(DaysTitleMaxLength),
			typeof(Calendar),
			DaysTitleMaxLength.ThreeChars,
			propertyChanged: OnDaysTitleMaximumLengthChanged
		);

	/// <summary>
	/// Specifies the maximum length of weekday titles
	/// </summary>
	public DaysTitleMaxLength DaysTitleMaximumLength
	{
		get => (DaysTitleMaxLength)GetValue(DaysTitleMaximumLengthProperty);
		set => SetValue(DaysTitleMaximumLengthProperty, value);
	}

	static void OnDaysTitleMaximumLengthChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}


	/// <summary>
	/// Bindable property for DaysTitleLabelFirstUpperRestLower
	/// </summary>
	public static readonly BindableProperty DaysTitleLabelFirstUpperRestLowerProperty = BindableProperty.Create(
			nameof(DaysTitleLabelFirstUpperRestLower),
			typeof(bool),
			typeof(Calendar),
			false,
			propertyChanged: OnDaysTitleLabelFirstUpperRestLowerChanged
		);

	/// <summary>
	/// Makes DaysTitleLabel text FirstCase Upper and rest lower
	/// </summary>
	public bool DaysTitleLabelFirstUpperRestLower
	{
		get => (bool)GetValue(DaysTitleLabelFirstUpperRestLowerProperty);
		set => SetValue(DaysTitleLabelFirstUpperRestLowerProperty, value);
	}

	static void OnDaysTitleLabelFirstUpperRestLowerChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}


	/// <summary>
	/// Bindable property for DaysTitleWeekendColor
	/// </summary>
	public static readonly BindableProperty DaysTitleWeekendColorProperty = BindableProperty.Create(
		nameof(DaysTitleWeekendColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black
	);

	/// <summary>
	/// Specifies the color for the titles of the weekend days
	/// </summary>
	public Color DaysTitleWeekendColor
	{
		get => (Color)GetValue(DaysTitleWeekendColorProperty);
		set => SetValue(DaysTitleWeekendColorProperty, value);
	}

	static void OnDaysTitleWeekendColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}

	#endregion

	#region SelectedDate BindableProperies

	/// <summary>
	/// Bindable property for SelectedDateColor
	/// </summary>
	public static readonly BindableProperty SelectedDateColorProperty = BindableProperty.Create(
		nameof(SelectedDateColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#2196F3")
	);

	/// <summary>
	/// Specifies the text color for the selected date
	/// </summary>
	public Color SelectedDateColor
	{
		get => (Color)GetValue(SelectedDateColorProperty);
		set => SetValue(SelectedDateColorProperty, value);
	}


	/// <summary>
	/// Bindable property for SelectedDayTextColor
	/// </summary>
	public static readonly BindableProperty SelectedDayTextColorProperty = BindableProperty.Create(
		nameof(SelectedDayTextColor),
		typeof(Color),
		typeof(Calendar),
		Colors.White,
		propertyChanged: OnSelectedDayTextColorChanged
	);

	/// <summary>
	/// Specifies the text color for the Selected days
	/// </summary>
	public Color SelectedDayTextColor
	{
		get => (Color)GetValue(SelectedDayTextColorProperty);
		set => SetValue(SelectedDayTextColorProperty, value);
	}

	static void OnSelectedDayTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for DeselectedDayTextColor
	/// </summary>
	public static readonly BindableProperty DeselectedDayTextColorProperty = BindableProperty.Create(
			nameof(DeselectedDayTextColor),
			typeof(Color),
			typeof(Calendar),
			Colors.Black,
			propertyChanged: OnDeselectedDayTextColorChanged
		);

	/// <summary>
	/// Specifies the text color for deselected days
	/// </summary>
	public Color DeselectedDayTextColor
	{
		get => (Color)GetValue(DeselectedDayTextColorProperty);
		set => SetValue(DeselectedDayTextColorProperty, value);
	}

	static void OnDeselectedDayTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for SelectedTodayTextColor
	/// </summary>
	public static readonly BindableProperty SelectedTodayTextColorProperty = BindableProperty.Create(
			nameof(SelectedTodayTextColor),
			typeof(Color),
			typeof(Calendar),
			Colors.Black,
			propertyChanged: OnSelectedTodayTextColorChanged
		);

	/// <summary>
	/// Specifies the text color of today's date when selected
	/// </summary>
	public Color SelectedTodayTextColor
	{
		get => (Color)GetValue(SelectedTodayTextColorProperty);
		set => SetValue(SelectedTodayTextColorProperty, value);
	}

	static void OnSelectedTodayTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for OtherMonthSelectedDayColor
	/// </summary>
	public static readonly BindableProperty OtherMonthSelectedDayColorProperty = BindableProperty.Create(
		nameof(OtherMonthSelectedDayColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Silver,
		propertyChanged: OnOtherMonthSelectedDayColorChanged
	);

	/// <summary>
	/// Specifies the color of selected days belonging to a month other than the selected one
	/// </summary>
	public Color OtherMonthSelectedDayColor
	{
		get => (Color)GetValue(OtherMonthSelectedDayColorProperty);
		set => SetValue(OtherMonthSelectedDayColorProperty, value);
	}

	static void OnOtherMonthSelectedDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for SelectedDayBackgroundColor
	/// </summary>
	public static readonly BindableProperty SelectedDayBackgroundColorProperty = BindableProperty.Create(
			nameof(SelectedDayBackgroundColor),
			typeof(Color),
			typeof(Calendar),
			Color.FromArgb("#2196F3"),
			propertyChanged: OnSelectedDayBackgroundColorChanged
		);

	/// <summary>
	/// Specifies the background color of selected days
	/// </summary>
	public Color SelectedDayBackgroundColor
	{
		get => (Color)GetValue(SelectedDayBackgroundColorProperty);
		set => SetValue(SelectedDayBackgroundColorProperty, value);
	}

	static void OnSelectedDayBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for SelectedDateText
	/// </summary>
	public static readonly BindableProperty SelectedDateTextProperty = BindableProperty.Create(
		nameof(SelectedDateText),
		typeof(string),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Text showing which dates are currently selected
	/// </summary>
	public string SelectedDateText
	{
		get => (string)GetValue(SelectedDateTextProperty);
		set => SetValue(SelectedDateTextProperty, value);
	}

	/// <summary>
	/// Bindable property for SelectedDateTextFormat
	/// </summary>
	public static readonly BindableProperty SelectedDateTextFormatProperty = BindableProperty.Create(
			nameof(SelectedDateTextFormat),
			typeof(string),
			typeof(Calendar),
			"d MMM yyyy"
		);

	/// <summary>
	/// Specifies the format of selected date text
	/// </summary>
	public string SelectedDateTextFormat
	{
		get => (string)GetValue(SelectedDateTextFormatProperty);
		set => SetValue(SelectedDateTextFormatProperty, value);
	}


	#endregion

	#region SelectedDates BindableProperies

	/// <summary>
	/// Bindable property for SelectedDate
	/// </summary>
	public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
		nameof(SelectedDate),
		typeof(DateTime?),
		typeof(Calendar),
		null,
		BindingMode.TwoWay,
		propertyChanged: OnSelectedDateChanged
	);
	/// <summary>
	/// Selected date in single date selection mode
	/// </summary>
	public DateTime? SelectedDate
	{
		get => (DateTime?)GetValue(SelectedDateProperty);
		set
		{
			SetValue(
				SelectedDatesProperty,
				value.HasValue ? new List<DateTime> { value.Value } : null
			);
			SetValue(SelectedDateProperty, value);
		}
	}
	static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var control = (Calendar)bindable;
		var dateToSet = (DateTime?)newValue;

		control.SetValue(SelectedDateProperty, dateToSet);
		if (
			!control.isSelectingDates
			|| control.CurrentSelectionEngine is SingleSelectionEngine
		)
		{
			if (dateToSet.HasValue)
			{
				control.SetValue(SelectedDatesProperty, new List<DateTime> { dateToSet.Value });
			}
			else
			{
				control.SetValue(SelectedDatesProperty, new List<DateTime>());
			}
		}
		else
		{
			control.isSelectingDates = false;
		}
		control.UpdateDays();

	}

	bool isSelectingDates = false;

	/// <summary>
	/// Bindable property for SelectedDates
	/// </summary>
	public static readonly BindableProperty SelectedDatesProperty = BindableProperty.Create(
		nameof(SelectedDates),
		typeof(List<DateTime>),
		typeof(Calendar),
		new List<DateTime>(),
		BindingMode.TwoWay,
		propertyChanged: SelectedDatesChanged
	);
	/// <summary>
	/// Selected date in single date selection mode
	/// </summary>
	public List<DateTime> SelectedDates
	{
		get => (List<DateTime>)GetValue(SelectedDatesProperty);
		set
		{
			SetValue(SelectedDatesProperty, value);
			isSelectingDates = true;
			SetValue(SelectedDateProperty, value?.Count > 0 ? value.First() : null);
		}
	}
	static void SelectedDatesChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar && (newValue is List<DateTime> || newValue is null) && !Equals(newValue, oldValue))
		{
			calendar.UpdateDays();
			calendar.CurrentSelectionEngine.UpdateDateSelection(calendar.SelectedDates);
			calendar.UpdateSelectedDateLabel();
			calendar.UpdateEvents();
		}
	}

	#endregion

	#region Swipe BindableProperties
	/// <summary>
	/// Bindable property for DisableSwipeDetection
	/// </summary>
	public static readonly BindableProperty SwipeDetectionDisabledProperty = BindableProperty.Create(
			 nameof(SwipeDetectionDisabled),
			 typeof(bool),
			 typeof(Calendar),
			 false
		 );

	public bool SwipeDetectionDisabled
	{
		get => (bool)GetValue(SwipeDetectionDisabledProperty);
		set => SetValue(SwipeDetectionDisabledProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeUpCommand
	/// </summary>
	public static readonly BindableProperty SwipeUpCommandProperty = BindableProperty.Create(
		nameof(SwipeUpCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Activated when user swipes-up over days view
	/// </summary>
	public ICommand SwipeUpCommand
	{
		get => (ICommand)GetValue(SwipeUpCommandProperty);
		set => SetValue(SwipeUpCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeUpToHideEnabled
	/// </summary>
	public static readonly BindableProperty SwipeUpToHideEnabledProperty = BindableProperty.Create(
		nameof(SwipeUpToHideEnabled),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Enable/disable default swipe-up action for showing/hiding calendar
	/// </summary>
	public bool SwipeUpToHideEnabled
	{
		get => (bool)GetValue(SwipeUpToHideEnabledProperty);
		set => SetValue(SwipeUpToHideEnabledProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeLeftCommand
	/// </summary>
	public static readonly BindableProperty SwipeLeftCommandProperty = BindableProperty.Create(
		nameof(SwipeLeftCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Activated when user swipes-left over days view
	/// </summary>
	public ICommand SwipeLeftCommand
	{
		get => (ICommand)GetValue(SwipeLeftCommandProperty);
		set => SetValue(SwipeLeftCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeRightCommand
	/// </summary>
	public static readonly BindableProperty SwipeRightCommandProperty = BindableProperty.Create(
		nameof(SwipeRightCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Activated when user swipes-right over days view
	/// </summary>
	public ICommand SwipeRightCommand
	{
		get => (ICommand)GetValue(SwipeRightCommandProperty);
		set => SetValue(SwipeRightCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeToChangeMonthEnabled
	/// </summary>
	public static readonly BindableProperty SwipeToChangeMonthEnabledProperty = BindableProperty.Create(
			nameof(SwipeToChangeMonthEnabled),
			typeof(bool),
			typeof(Calendar),
			true
		);

	/// <summary>
	/// Enable/disable default swipe actions for changing months
	/// </summary>
	public bool SwipeToChangeMonthEnabled
	{
		get => (bool)GetValue(SwipeToChangeMonthEnabledProperty);
		set => SetValue(SwipeToChangeMonthEnabledProperty, value);
	}
	#endregion


	void InitializeSelectionType()
	{
		CurrentSelectionEngine = new SingleSelectionEngine();
	}

	#region Properties

	/// <summary>
	/// When executed calendar moves to previous week/month.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand PrevLayoutUnitCommand { get; }

	/// <summary>
	/// When executed calendar moves to next week/month.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand NextLayoutUnitCommand { get; }

	/// <summary>
	/// When executed calendar moves to previous year.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand PrevYearCommand { get; }

	/// <summary>
	/// When executed calendar moves to next year.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand NextYearCommand { get; }

	/// <summary>
	/// When executed shows/hides the calendar's current month days view.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand ShowHideCalendarCommand { get; }

	#endregion

	#region Update methods
	void UpdateEvents()
	{
		SelectedDayEvents =
			CurrentSelectionEngine.TryGetSelectedEvents(Events, out var selectedEvents)
				? selectedEvents
				: null;

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

	void UpdateSelectedDateLabel() =>
		 SelectedDateText = CurrentSelectionEngine.GetSelectedDateText(SelectedDateTextFormat, Culture);


	void ShowHideCalendarSection()
	{
		if (calendarSectionAnimating)
		{
			return;
		}

		calendarSectionAnimating = true;

		var animation = CalendarSectionShown
			? calendarSectionAnimateShow
			: calendarSectionAnimateHide;
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
	}

	void OnDayModelPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (
			e.PropertyName != nameof(DayModel.IsSelected)
			|| sender is not DayModel newSelected
			|| (
				propertyChangedNotificationSupressions.TryGetValue(
					e.PropertyName,
					out bool isSuppressed
				) && isSuppressed
			)
		)
		{
			return;
		}

		SelectedDates = CurrentSelectionEngine.PerformDateSelection(newSelected.Date, DisabledDates);
	}

	void UpdateDayTitles()
	{
		var dayNumber = (int)FirstDayOfWeek;

		foreach (var dayLabel in daysControl.Children.OfType<Label>())
		{
			var abberivatedDayName = Culture.DateTimeFormat.AbbreviatedDayNames[dayNumber];
			var titleText = DaysTitleLabelFirstUpperRestLower
				? abberivatedDayName[..1].ToUpperInvariant()
					+ abberivatedDayName[1..].ToLowerInvariant()
				: abberivatedDayName.ToUpperInvariant();
			dayLabel.Text = titleText[
				..(
					(int)DaysTitleMaximumLength > abberivatedDayName.Length
						? abberivatedDayName.Length
						: (int)DaysTitleMaximumLength
				)
			];
			// Detect weekend days
			if (
				DaysTitleColor != DaysTitleWeekendColor
				&& (dayNumber == (int)DayOfWeek.Saturday || dayNumber == (int)DayOfWeek.Sunday)
			)
			{
				// It's a weekend day
				// You can change the color of the label or do something else
				dayLabel.TextColor = DaysTitleWeekendColor;
			}
			dayNumber = (dayNumber + 1) % 7;
		}
	}

	void UpdateDays()
	{
		var firstDate = CurrentViewLayoutEngine.GetFirstDate(ShownDate);

		int addDays = 0;
		foreach (var dayView in dayViews)
		{
			var currentDate = firstDate.AddDays(addDays++);
			var dayModel = dayView.BindingContext as DayModel;

			dayModel.Date = currentDate.Date;
			dayModel.DayTappedCommand = DayTappedCommand;
			dayModel.EventIndicatorType = EventIndicatorType;
			dayModel.DayViewSize = DayViewSize;
			dayModel.FontSize = DayViewFontSize;
			dayModel.DayViewCornerRadius = DayViewCornerRadius;
			dayModel.DaysLabelStyle = DaysLabelStyle;
			dayModel.IsThisMonth =
				(CalendarLayout != WeekLayout.Month) || currentDate.Month == ShownDate.Month;
			dayModel.OtherMonthIsVisible =
				(CalendarLayout != WeekLayout.Month) || OtherMonthDayIsVisible;
			dayModel.HasEvents = Events.ContainsKey(currentDate);
			dayModel.IsDisabled =
				currentDate < MinimumDate
				|| currentDate > MaximumDate
				|| (DisabledDates?.Contains(currentDate.Date) ?? false);
			dayModel.AllowDeselect = AllowDeselecting;

			ChangePropertySilently(
				nameof(dayModel.IsSelected),
				() => dayModel.IsSelected = CurrentSelectionEngine.IsDateSelected(dayModel.Date)
			);
			AssignIndicatorColors(ref dayModel);
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
			dayModel.FontSize = DayViewFontSize;

			AssignIndicatorColors(ref dayModel);
		}
	}

	#endregion

	#region Event Handlers

	void OnCalendarContainerSizeChanged(object sender, EventArgs e)
	{
		if (calendarContainer.Height > 0 && !calendarSectionAnimating)
		{
			UpdateCalendarSectionHeight();
		}
	}

	void OnSwipedRight(object sender, EventArgs e)
	{
		SwipeRightCommand?.Execute(null);

		if (SwipeToChangeMonthEnabled)
		{
			PrevUnit();
		}
	}

	void OnSwipedLeft(object sender, EventArgs e)
	{
		SwipeLeftCommand?.Execute(null);

		if (SwipeToChangeMonthEnabled)
		{
			NextUnit();
		}
	}

	void OnSwipedUp(object sender, EventArgs e)
	{
		SwipeUpCommand?.Execute(null);

		if (SwipeUpToHideEnabled)
		{
			ToggleCalendarSectionVisibility();
		}
	}

	#endregion


	#region Other methods

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

	void PrevYear(object obj)
	{
		ShownDate = ShownDate.AddYears(-1);
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

		daysControl = CurrentViewLayoutEngine.GenerateLayout(
			dayViews,
			this,
			nameof(DaysTitleColor),
			nameof(DaysTitleLabelStyle),
			DayTappedCommand,
			OnDayModelPropertyChanged
		);

		UpdateDaysColors();
		UpdateDayTitles();

		calendarContainer.Add(daysControl);
	}

	void DiposeDayViews()
	{

		foreach (var dayView in daysControl.Children.OfType<DayView>())
		{
			if (dayView.BindingContext is DayModel dayModel)
			{
				dayModel.PropertyChanged -= OnDayModelPropertyChanged;
#if !WINDOWS
				dayView.BindingContext = null;
#endif
			}
		}
	}

	internal void ChangePropertySilently(string propertyName, Action propertyChangeAction)
	{
		propertyChangedNotificationSupressions[propertyName] = true;
		propertyChangeAction();
		propertyChangedNotificationSupressions[propertyName] = false;
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
				dayModel.EventColors = multiEventDay.Colors?.ToArray() ?? [];
			}
			else
			{
				dayModel.EventColors = [];
			}
		}
		else
		{
			dayModel.EventColors = [];
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Events is EventCollection events)
			{
				events.CollectionChanged -= OnEventsCollectionChanged;
			}
			DiposeDayViews();
			calendarSectionAnimateHide.Value.Dispose();
			calendarSectionAnimateShow.Value.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	#endregion
}