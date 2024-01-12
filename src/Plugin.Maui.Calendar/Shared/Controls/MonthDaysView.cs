using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class MonthDaysView : ContentView
{
    SwipeGestureRecognizer leftSwipeGesture;
    SwipeGestureRecognizer rightSwipeGesture;
    SwipeGestureRecognizer upSwipeGesture;
    SwipeGestureRecognizer downSwipeGesture;

    public static readonly BindableProperty SwipeDetectionDisabledProperty =
      BindableProperty.Create(nameof(SwipeDetectionDisabled), typeof(bool), typeof(MonthDaysView), false);

    public bool SwipeDetectionDisabled
    {
        get => (bool)GetValue(SwipeDetectionDisabledProperty);
        set => SetValue(SwipeDetectionDisabledProperty, value);
    }

    private Grid _daysControl;

    #region Bindable Properties

    /// <summary>
    /// Bindable property for ShownDate
    /// </summary>
    public static readonly BindableProperty ShownDateProperty =
      BindableProperty.Create(nameof(ShownDate), typeof(DateTime), typeof(MonthDaysView), DateTime.Today, BindingMode.TwoWay);

    /// <summary>
    /// Currently displayed month of selected year
    /// </summary>
    public DateTime ShownDate
    {
        get => (DateTime)GetValue(ShownDateProperty);
        set => SetValue(ShownDateProperty, value);
    }

    /// <summary>
    /// Bindable property for SelectedDates
    /// </summary>
    public static readonly BindableProperty SelectedDatesProperty =
      BindableProperty.Create(nameof(SelectedDates), typeof(List<DateTime>), typeof(MonthDaysView), new List<DateTime>(), BindingMode.TwoWay, propertyChanged: SelectedDatesChanged);

    private static void SelectedDatesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MonthDaysView control && (newValue is List<DateTime> || newValue is null) && !Equals(newValue, oldValue))
            control.UpdateDays();
    }

    /// <summary>
    /// Selected date in single date selection mode
    /// </summary>
    public List<DateTime> SelectedDates
    {
        get => (List<DateTime>)GetValue(SelectedDatesProperty);
        set => SetValue(SelectedDatesProperty, value);
    }

    /// <summary>
    /// Bindable property for Culture
    /// </summary>
    public static readonly BindableProperty CultureProperty =
      BindableProperty.Create(nameof(Culture), typeof(CultureInfo), typeof(MonthDaysView), CultureInfo.InvariantCulture, BindingMode.TwoWay);

    /// <summary>
    /// Culture info to properly format and name days
    /// </summary>
    public CultureInfo Culture
    {
        get => (CultureInfo)GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    /// <summary>
    /// Bindable property for Events
    /// </summary>
    public static readonly BindableProperty EventsProperty =
      BindableProperty.Create(nameof(Events), typeof(EventCollection), typeof(MonthDaysView), new EventCollection());

    /// <summary>
    /// Collection of all the events on the calendar
    /// </summary>
    public EventCollection Events
    {
        get => (EventCollection)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }

    /// <summary>
    /// Bindable property for DaysTitleWeekendColor
    /// </summary>
    public static readonly BindableProperty DaysTitleWeekendColorProperty =
      BindableProperty.Create(nameof(DaysTitleWeekendColor), typeof(Color), typeof(MonthDaysView), null);

    /// <summary>
    /// Color of weekday titles
    /// </summary>
    public Color DaysTitleWeekendColor
    {
        get => (Color)GetValue(DaysTitleWeekendColorProperty);
        set => SetValue(DaysTitleWeekendColorProperty, value);
    }

    /// <summary>
    /// Bindable property for OtherMonthDayIsVisible
    /// </summary>
    public static readonly BindableProperty OtherMonthDayIsVisibleProperty =
      BindableProperty.Create(nameof(OtherMonthDayIsVisible), typeof(bool), typeof(MonthDaysView), true);

    /// <summary>
    /// Specifying if days from other months are visible on the current month view
    /// </summary>
    public bool OtherMonthDayIsVisible
    {
        get => (bool)GetValue(OtherMonthDayIsVisibleProperty);
        set => SetValue(OtherMonthDayIsVisibleProperty, value);
    }

    /// <summary>
    /// Bindable property for SelectedDayBackgroundColor
    /// </summary>
    public static readonly BindableProperty SelectedDayBackgroundColorProperty =
      BindableProperty.Create(nameof(SelectedDayBackgroundColor), typeof(Color), typeof(MonthDaysView), Color.FromArgb("#2196F3"));

    /// <summary>
    /// Background color of currently selected date
    /// </summary>
    public Color SelectedDayBackgroundColor
    {
        get => (Color)GetValue(SelectedDayBackgroundColorProperty);
        set => SetValue(SelectedDayBackgroundColorProperty, value);
    }

    /// <summary>
    /// Bindable property for EventIndicatorColor
    /// </summary>
    public static readonly BindableProperty EventIndicatorTypeProperty =
      BindableProperty.Create(nameof(EventIndicatorType), typeof(EventIndicatorType), typeof(MonthDaysView), EventIndicatorType.BottomDot);

    /// <summary>
    /// Enum value specifying the way events are indicated on dates
    /// </summary>
    public EventIndicatorType EventIndicatorType
    {
        get => (EventIndicatorType)GetValue(EventIndicatorTypeProperty);
        set => SetValue(EventIndicatorTypeProperty, value);
    }

    /// <summary>
    /// Bindable property for EventIndicatorColor
    /// </summary>
    public static readonly BindableProperty EventIndicatorColorProperty =
      BindableProperty.Create(nameof(EventIndicatorColor), typeof(Color), typeof(MonthDaysView), Color.FromArgb("#FF4081"));

    /// <summary>
    /// Color of event indicator on dates
    /// </summary>
    public Color EventIndicatorColor
    {
        get => (Color)GetValue(EventIndicatorColorProperty);
        set => SetValue(EventIndicatorColorProperty, value);
    }

    /// <summary>
    /// Bindable property for EventIndicatorSelectedColor
    /// </summary>
    public static readonly BindableProperty EventIndicatorSelectedColorProperty =
      BindableProperty.Create(nameof(EventIndicatorSelectedColor), typeof(Color), typeof(MonthDaysView), Color.FromArgb("#FF4081"));

    /// <summary>
    /// Color of event indicator on selected dates
    /// </summary>
    public Color EventIndicatorSelectedColor
    {
        get => (Color)GetValue(EventIndicatorSelectedColorProperty);
        set => SetValue(EventIndicatorSelectedColorProperty, value);
    }


    /// <summary>
    /// Bindable property for TodayOutlineColor
    /// </summary>
    public static readonly BindableProperty TodayOutlineColorProperty =
      BindableProperty.Create(nameof(TodayOutlineColor), typeof(Color), typeof(MonthDaysView), Color.FromArgb("#FF4081"));

    /// <summary>
    /// Color of today date's outline
    /// </summary>
    public Color TodayOutlineColor
    {
        get => (Color)GetValue(TodayOutlineColorProperty);
        set => SetValue(TodayOutlineColorProperty, value);
    }

    /// <summary>
    /// Bindable property for TodayFillColor
    /// </summary>
    public static readonly BindableProperty TodayFillColorProperty =
      BindableProperty.Create(nameof(TodayFillColor), typeof(Color), typeof(MonthDaysView), Colors.Black);

    /// <summary>
    /// Color of today date's fill
    /// </summary>
    public Color TodayFillColor
    {
        get => (Color)GetValue(TodayFillColorProperty);
        set => SetValue(TodayFillColorProperty, value);
    }

    /// <summary>
    /// Bindable property for DayViewSize
    /// </summary>
    public static readonly BindableProperty DayViewSizeProperty =
      BindableProperty.Create(nameof(DayViewSize), typeof(double), typeof(MonthDaysView), 40.0);

    /// <summary>
    /// Size of all individual dates
    /// </summary>
    public double DayViewSize
    {
        get => (double)GetValue(DayViewSizeProperty);
        set => SetValue(DayViewSizeProperty, value);
    }

    /// <summary>
    /// Bindable property for DayViewCornerRadius
    /// </summary>
    public static readonly BindableProperty DayViewCornerRadiusProperty =
      BindableProperty.Create(nameof(DayViewCornerRadius), typeof(float), typeof(MonthDaysView), 20f);

    /// <summary>
    /// Corner radius of individual dates
    /// </summary>
    public float DayViewCornerRadius
    {
        get => (float)GetValue(DayViewCornerRadiusProperty);
        set => SetValue(DayViewCornerRadiusProperty, value);
    }

    /// <summary>
    /// Bindable property for DaysTitleHeight
    /// </summary>
    public static readonly BindableProperty DaysTitleHeightProperty =
      BindableProperty.Create(nameof(DaysTitleHeight), typeof(double), typeof(MonthDaysView), 30.0);

    /// <summary>
    /// Height of the weekday names container
    /// </summary>
    public double DaysTitleHeight
    {
        get => (double)GetValue(DaysTitleHeightProperty);
        set => SetValue(DaysTitleHeightProperty, value);
    }

    /// <summary>
    /// Bindable property for DaysTitleMaximumLength
    /// </summary>
    public static readonly BindableProperty DaysTitleMaximumLengthProperty =
    BindableProperty.Create(nameof(DaysTitleMaximumLength), typeof(DaysTitleMaxLength), typeof(MonthDaysView), DaysTitleMaxLength.ThreeChars);

    /// <summary>
    /// Maximum character length of weekday titles
    /// </summary>
    public DaysTitleMaxLength DaysTitleMaximumLength
    {
        get => (DaysTitleMaxLength)GetValue(DaysTitleMaximumLengthProperty);
        set => SetValue(DaysTitleMaximumLengthProperty, value);
    }

    public Style DayLabelStyle
    {
        get => (Style)GetValue(DayLabelStyleProperty);
        set => SetValue(DayLabelStyleProperty, value);
    }

    public static readonly BindableProperty DayLabelStyleProperty =
      BindableProperty.Create(nameof(DayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultLabelStyle, propertyChanged: StylePropertyChanges);

    public Style OtherMonthDaysLabelStyle
    {
        get => (Style)GetValue(OtherMonthDaysLabelStyleProperty);
        set => SetValue(OtherMonthDaysLabelStyleProperty, value);
    }

    public static readonly BindableProperty OtherMonthDaysLabelStyleProperty =
      BindableProperty.Create(nameof(OtherMonthDaysLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultOtherMonthDayLabelStyle, propertyChanged: StylePropertyChanges);

    public Style DeselectedDayLabelStyle
    {
        get => (Style)GetValue(DeselectedDayLabelStyleProperty);
        set => SetValue(DeselectedDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty DeselectedDayLabelStyleProperty =
      BindableProperty.Create(nameof(DeselectedDayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultDeselectedDayLabelStyle, propertyChanged: StylePropertyChanges);

    public Style WeekendDayLabelStyle
    {
        get => (Style)GetValue(WeekendDayLabelStyleProperty);
        set => SetValue(WeekendDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty WeekendDayLabelStyleProperty =
      BindableProperty.Create(nameof(WeekendDayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultWeekendDayLabelStyle, propertyChanged: StylePropertyChanges);

    public Style DisabledDayLabelStyle
    {
        get => (Style)GetValue(DisabledDayLabelStyleProperty);
        set => SetValue(DisabledDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty DisabledDayLabelStyleProperty =
      BindableProperty.Create(nameof(DisabledDayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultDisabledDayLabelStyle, propertyChanged: StylePropertyChanges);

    public Style SelectedTodayLabelStyle
    {
        get => (Style)GetValue(SelectedTodayLabelStyleProperty);
        set => SetValue(SelectedTodayLabelStyleProperty, value);
    }

    public static readonly BindableProperty SelectedTodayLabelStyleProperty =
      BindableProperty.Create(nameof(SelectedTodayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultSelectedTodayLabelStyle, propertyChanged: StylePropertyChanges);

    public Style SelectedDayLabelStyle
    {
        get => (Style)GetValue(SelectedDayLabelStyleProperty);
        set => SetValue(SelectedDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty SelectedDayLabelStyleProperty =
      BindableProperty.Create(nameof(SelectedDayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultSelectedDayLabelStyle, propertyChanged: StylePropertyChanges);

    public Style TodayLabelStyle
    {
        get => (Style)GetValue(TodayLabelStyleProperty);
        set => SetValue(TodayLabelStyleProperty, value);
    }

    public static readonly BindableProperty TodayLabelStyleProperty =
      BindableProperty.Create(nameof(TodayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultTodayLabelStyle, propertyChanged: StylePropertyChanges);

    public Style EventIndicatorSelectedLabelStyle
    {
        get => (Style)GetValue(EventIndicatorSelectedLabelStyleProperty);
        set => SetValue(EventIndicatorSelectedLabelStyleProperty, value);
    }

    public static readonly BindableProperty EventIndicatorSelectedLabelStyleProperty =
      BindableProperty.Create(nameof(EventIndicatorSelectedLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultEventIndicatorSelectedLabelStyle, propertyChanged: StylePropertyChanges);

    public Style EventIndicatorLabelStyle
    {
        get => (Style)GetValue(EventIndicatorLabelStyleProperty);
        set => SetValue(EventIndicatorLabelStyleProperty, value);
    }

    public static readonly BindableProperty EventIndicatorLabelStyleProperty =
      BindableProperty.Create(nameof(EventIndicatorLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultEventIndicatorLabelStyle, propertyChanged: StylePropertyChanges);


    /// <summary>
    /// Bindable property for DaysTitleLabelFirstUpperRestLower
    /// </summary>
    public static readonly BindableProperty DaysTitleLabelFirstUpperRestLowerProperty =
      BindableProperty.Create(nameof(DaysTitleLabelFirstUpperRestLower), typeof(bool), typeof(MonthDaysView), false);

    /// <summary>
    /// Makes DaysTitleLabel text FirstCase Upper and rest lower
    /// </summary>
    public bool DaysTitleLabelFirstUpperRestLower
    {
        get => (bool)GetValue(DaysTitleLabelFirstUpperRestLowerProperty);
        set => SetValue(DaysTitleLabelFirstUpperRestLowerProperty, value);
    }

    /// <summary>
    /// Bindable property for DaysTitleLabelStyle
    /// </summary>
    public static readonly BindableProperty DaysTitleLabelStyleProperty =
      BindableProperty.Create(nameof(DaysTitleLabelStyle), typeof(Style), typeof(MonthDaysView), defaultValue: DefaultStyles.DefaultTitleDaysLabelStyle, propertyChanged: StylePropertyChanges);

    private static void StylePropertyChanges(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MonthDaysView control && (newValue is Style || newValue is null) && !Equals(newValue, oldValue))
            control.UpdateDays();
    }

    /// <summary>
    /// ???
    /// </summary>
    public Style DaysTitleLabelStyle
    {
        get => (Style)GetValue(DaysTitleLabelStyleProperty);
        set => SetValue(DaysTitleLabelStyleProperty, value);
    }

    /// <summary>
    /// Bindable property for DayTapped
    /// </summary>
    public static readonly BindableProperty DayTappedCommandProperty =
        BindableProperty.Create(nameof(DayTappedCommand), typeof(ICommand), typeof(MonthDaysView), null);

    /// <summary>
    /// Action to run after a day has been tapped.
    /// </summary>
    public ICommand DayTappedCommand
    {
        get => (ICommand)GetValue(DayTappedCommandProperty);
        set => SetValue(DayTappedCommandProperty, value);
    }

    /// <summary>
    /// Bindable property for MinimumDate
    /// </summary>
    public static readonly BindableProperty MinimumDateProperty =
      BindableProperty.Create(nameof(MinimumDate), typeof(DateTime), typeof(MonthDaysView), DateTime.MinValue);

    /// <summary>
    /// Minimum date which can be selected
    /// </summary>
    public DateTime MinimumDate
    {
        get => (DateTime)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    /// <summary>
    /// Bindable property for MaximumDate
    /// </summary>
    public static readonly BindableProperty MaximumDateProperty =
      BindableProperty.Create(nameof(MaximumDate), typeof(DateTime), typeof(MonthDaysView), DateTime.MaxValue);

    /// <summary>
    /// Maximum date which can be selected
    /// </summary>
    public DateTime MaximumDate
    {
        get => (DateTime)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    /// <summary>
    /// Bindable property for AnimateCalendar
    /// </summary>
    public static readonly BindableProperty AnimateCalendarProperty =
        BindableProperty.Create(nameof(AnimateCalendar), typeof(bool), typeof(MonthDaysView), true);

    /// <summary>
    /// Specifies if the calendar should animate or not
    /// </summary>
    public bool AnimateCalendar
    {
        get => (bool)GetValue(AnimateCalendarProperty);
        set => SetValue(AnimateCalendarProperty, value);
    }

    /// <summary>
    /// Bindable property for WeekLayout
    /// </summary>
    public static readonly BindableProperty CalendarLayoutProperty =
        BindableProperty.Create(nameof(CalendarLayout), typeof(WeekLayout), typeof(MonthDaysView), WeekLayout.Month);

    /// <summary>
    /// Sets the layout of the calendar
    /// </summary>
    public WeekLayout CalendarLayout
    {
        get => (WeekLayout)GetValue(CalendarLayoutProperty);
        set => SetValue(CalendarLayoutProperty, value);
    }

    #endregion

    /// <summary>
    /// Current implementation of selection engine
    /// </summary>
    internal ISelectionEngine CurrentSelectionEngine { get; set; } = new SingleSelectionEngine();

    internal IViewLayoutEngine CurrentViewLayoutEngine { get; set; } = new MonthViewEngine(CultureInfo.InvariantCulture);

    private readonly Dictionary<string, bool> _propertyChangedNotificationSupressions = new();
    private readonly List<DayView> _dayViews = [];
    private DateTime _lastAnimationTime;
    private bool _animating;

    public MonthDaysView()
    {
        if (!SwipeDetectionDisabled)
        {
            leftSwipeGesture = new() { Direction = SwipeDirection.Left };
            rightSwipeGesture = new() { Direction = SwipeDirection.Right };
            upSwipeGesture = new() { Direction = SwipeDirection.Up };
            downSwipeGesture = new() { Direction = SwipeDirection.Down };

            Loaded += LoadedMethod;
            Unloaded += UnloadedMethod;
        }

        RenderLayout();
    }

    ~MonthDaysView() => DiposeDayViews();


    private void LoadedMethod(object sender, EventArgs e)
    {
        if (!SwipeDetectionDisabled)
        {
            GestureRecognizers.Add(leftSwipeGesture);
            GestureRecognizers.Add(rightSwipeGesture);
            GestureRecognizers.Add(upSwipeGesture);
            GestureRecognizers.Add(downSwipeGesture);

            leftSwipeGesture.Swiped += OnSwiped;
            rightSwipeGesture.Swiped += OnSwiped;
            upSwipeGesture.Swiped += OnSwiped;
            downSwipeGesture.Swiped += OnSwiped;
        }

    }

    private void UnloadedMethod(object sender, EventArgs e)
    {
        if (!SwipeDetectionDisabled && GestureRecognizers.Count > 0)
        {
            GestureRecognizers.Remove(leftSwipeGesture);
            GestureRecognizers.Remove(rightSwipeGesture);
            GestureRecognizers.Remove(upSwipeGesture);
            GestureRecognizers.Remove(downSwipeGesture);

            leftSwipeGesture.Swiped -= OnSwiped;
            rightSwipeGesture.Swiped -= OnSwiped;
            upSwipeGesture.Swiped -= OnSwiped;
            downSwipeGesture.Swiped -= OnSwiped;
        }
    }

    #region PropertyChanged

    /// <summary>
    /// Method that is called when a bound property is changed.
    /// </summary>
    /// <param name="propertyName">The name of the bound property that changed.</param>
    protected async override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (_propertyChangedNotificationSupressions.TryGetValue(propertyName, out bool isSuppressed)
            && isSuppressed)
            return;

        switch (propertyName)
        {
            case nameof(SelectedDates):
                CurrentSelectionEngine.UpdateDateSelection(SelectedDates);
                break;
            case nameof(Events):
            case nameof(ShownDate):
            case nameof(MinimumDate):
            case nameof(MaximumDate):
            case nameof(OtherMonthDayIsVisible):
                await UpdateAndAnimateDays(AnimateCalendar);
                break;
            case nameof(SelectedDayBackgroundColor):
            case nameof(EventIndicatorColor):
            case nameof(EventIndicatorSelectedColor):
            case nameof(EventIndicatorType):
            case nameof(TodayOutlineColor):
            case nameof(TodayFillColor):
                UpdateDaysColors();
                break;
            case nameof(Culture):
                RenderLayout();
                await UpdateAndAnimateDays(AnimateCalendar);
                break;

            case nameof(DaysTitleMaximumLength):
            case nameof(DaysTitleLabelStyle):
            case nameof(DaysTitleHeight):
            case nameof(DaysTitleWeekendColor):
            case nameof(DaysTitleLabelFirstUpperRestLower):
                UpdateDayTitles();
                break;

            case nameof(CalendarLayout):
                RenderLayout();
                break;
        }
    }

    private void OnDayModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DayModel.IsSelected) || sender is not DayModel newSelected ||
            (_propertyChangedNotificationSupressions.TryGetValue(e.PropertyName, out bool isSuppressed) && isSuppressed))
            return;

        SelectedDates = CurrentSelectionEngine.PerformDateSelection(newSelected.Date);
    }

    private void UpdateDayTitles()
    {
        var dayNumber = (int)Culture.DateTimeFormat.FirstDayOfWeek;

        foreach (var dayLabel in _daysControl.Children.OfType<Label>())
        {
            var abberivatedDayName = Culture.DateTimeFormat.AbbreviatedDayNames[dayNumber];
            var titleText = DaysTitleLabelFirstUpperRestLower ? abberivatedDayName[..1].ToUpperInvariant() + abberivatedDayName[1..].ToLowerInvariant() : abberivatedDayName.ToUpper();
            dayLabel.Text = titleText[..((int)DaysTitleMaximumLength > abberivatedDayName.Length ? abberivatedDayName.Length : (int)DaysTitleMaximumLength)];

            dayLabel.Style = DaysTitleLabelStyle;
            // Detect weekend days
            if (DaysTitleWeekendColor != null && (dayNumber == (int)DayOfWeek.Saturday || dayNumber == (int)DayOfWeek.Sunday))
            {
                // It's a weekend day
                // You can change the color of the label or do something else
                dayLabel.TextColor = DaysTitleWeekendColor;
            }

            dayNumber = (dayNumber + 1) % 7;
        }
    }

    internal async Task UpdateAndAnimateDays(bool animate)
    {
        if (Culture == null)
            return;

        _lastAnimationTime = DateTime.UtcNow;

        if (BindingContext == null)
        {
            UpdateDays();
        }
        else
        {
            await Animate(() => _daysControl.FadeTo(animate ? 0 : 1, 50),
                    () => _daysControl.FadeTo(1, 200),
                    () => UpdateDays(),
                    _lastAnimationTime,
                    async () => await UpdateAndAnimateDays(false)); //send false to prevent flashing if several property bindings are changed
        }
    }

    private void UpdateDays()
    {
        var firstDate = CurrentViewLayoutEngine.GetFirstDate(ShownDate);

        int addDays = 0;
        foreach (var dayView in _dayViews)
        {
            var currentDate = firstDate.AddDays(addDays++);
            var dayModel = dayView.BindingContext as DayModel;

            dayModel.Date = currentDate.Date;
            dayModel.DayTappedCommand = DayTappedCommand;
            dayModel.EventIndicatorType = EventIndicatorType;
            dayModel.DayViewSize = DayViewSize;
            dayModel.DayViewCornerRadius = DayViewCornerRadius;
            dayModel.IsThisMonth = (CalendarLayout != WeekLayout.Month) || currentDate.Month == ShownDate.Month;
            dayModel.DaysLabelStyle = GetDayLabelStyle(dayModel);
            dayModel.OtherMonthIsVisible = (CalendarLayout != WeekLayout.Month) || OtherMonthDayIsVisible;
            dayModel.HasEvents = Events.ContainsKey(currentDate);
            dayModel.IsDisabled = currentDate < MinimumDate || currentDate > MaximumDate;

            ChangePropertySilently(nameof(dayModel.IsSelected), () => dayModel.IsSelected = CurrentSelectionEngine.IsDateSelected(dayModel.Date));
            AssignIndicatorColors(ref dayModel);
        }
    }

    Style GetDayLabelStyle(DayModel dayModel)
    {
        if (!dayModel.IsVisible) return OtherMonthDaysLabelStyle;

        return (dayModel.IsDisabled, dayModel.IsSelected, dayModel.HasEvents, dayModel.IsThisMonth, dayModel.IsToday, dayModel.IsWeekend) switch
        {
            (true, _, _, _, _, _) => DisabledDayLabelStyle,
            (false, true, false, true, true, _) => SelectedTodayLabelStyle.GetSetterValue<Color>(Label.TextColorProperty) == Colors.Transparent ? SelectedDayLabelStyle : SelectedTodayLabelStyle,
            (false, true, false, true, false, _) => SelectedDayLabelStyle,
            (false, true, true, true, _, _) => EventIndicatorSelectedLabelStyle,
            (false, false, true, true, _, _) => EventIndicatorLabelStyle,
            (false, false, _, false, _, _) => OtherMonthDaysLabelStyle,
            (false, false, false, true, true, _) => TodayLabelStyle,
            (false, _, _, _, _, true) => WeekendDayLabelStyle,
            (false, false, false, true, false, _) => DeselectedDayLabelStyle,
            (_, _, _, _, _, _) => DayLabelStyle
        };
    }

    private void UpdateDaysColors()
    {
        foreach (var dayView in _dayViews)
        {
            var dayModel = dayView.BindingContext as DayModel;

            dayModel.SelectedBackgroundColor = SelectedDayBackgroundColor;
            dayModel.TodayOutlineColor = TodayOutlineColor;
            dayModel.TodayFillColor = TodayFillColor;

            AssignIndicatorColors(ref dayModel);
        }
    }

    #endregion

    private void RenderLayout()
    {
        CurrentViewLayoutEngine = CalendarLayout switch
        {
            WeekLayout.Week => new WeekViewEngine(Culture, 1),
            WeekLayout.TwoWeek => new WeekViewEngine(Culture, 2),
            _ => new MonthViewEngine(Culture),
        };

        _daysControl = CurrentViewLayoutEngine.GenerateLayout(
            _dayViews,
            this,
            nameof(DaysTitleHeight),
            nameof(DaysTitleLabelStyle),
            nameof(DayViewSize),
            DayTappedCommand,
            OnDayModelPropertyChanged);

        UpdateDaysColors();
        UpdateDayTitles();

        Content = _daysControl;
    }

    private void DiposeDayViews()
    {
        foreach (var dayView in _daysControl.Children.OfType<DayView>())
        {
            (dayView.BindingContext as DayModel).PropertyChanged -= OnDayModelPropertyChanged;
#if !WINDOWS
            dayView.BindingContext = null;
#endif
        }

        Loaded -= LoadedMethod;
        Unloaded -= UnloadedMethod;

        leftSwipeGesture = null;
        rightSwipeGesture = null;
        upSwipeGesture = null;
        downSwipeGesture = null;
    }

    private async Task Animate(
    Func<Task> animationIn,
    Func<Task> animationOut,
    Action afterFirstAnimation,
    DateTime animationTime,
    Action callAgain)
    {
        if (_animating)
            return;

        _animating = true;
        await animationIn();
        afterFirstAnimation();
        await animationOut();
        _animating = false;
        if (animationTime != _lastAnimationTime)
            callAgain();
    }

    internal void ChangePropertySilently(string propertyName, Action propertyChangeAction)
    {
        _propertyChangedNotificationSupressions[propertyName] = true;
        propertyChangeAction();
        _propertyChangedNotificationSupressions[propertyName] = false;
    }



    internal void AssignIndicatorColors(ref DayModel dayModel)
    {
        if (Events.TryGetValue(dayModel.Date, out var dayEventCollection) && dayEventCollection is IPersonalizableDayEvent personalizableDay)
        {
            dayModel.EventIndicatorColor = personalizableDay?.EventIndicatorColor ?? EventIndicatorColor;
            dayModel.EventIndicatorSelectedColor = personalizableDay?.EventIndicatorSelectedColor ?? personalizableDay?.EventIndicatorColor ?? EventIndicatorSelectedColor;
            //dayModel.EventIndicatorTextColor = personalizableDay?.EventIndicatorTextColor ?? EventIndicatorTextColor;
            // dayModel.EventIndicatorSelectedTextColor = personalizableDay?.EventIndicatorSelectedTextColor ?? personalizableDay?.EventIndicatorTextColor ?? EventIndicatorSelectedTextColor;
        }
        else
        {
            dayModel.EventIndicatorColor = EventIndicatorColor;
            dayModel.EventIndicatorSelectedColor = EventIndicatorSelectedColor;
            //dayModel.EventIndicatorTextColor = EventIndicatorTextColor;
            // dayModel.EventIndicatorSelectedTextColor = EventIndicatorSelectedTextColor;
        }
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

    public event EventHandler SwipedLeft;
    public event EventHandler SwipedRight;
    public event EventHandler SwipedUp;
    public event EventHandler SwipedDown;

    void OnSwipeLeft() => SwipedLeft?.Invoke(this, EventArgs.Empty);
    void OnSwipeRight() => SwipedRight?.Invoke(this, EventArgs.Empty);
    void OnSwipeUp() => SwipedUp?.Invoke(this, EventArgs.Empty);
    void OnSwipeDown() => SwipedDown?.Invoke(this, EventArgs.Empty);
}
