using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Styles;

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

    //////////////////////Disabled dates//////////////////////////////

    public static readonly BindableProperty DisabledDatesProperty =
      BindableProperty.Create(nameof(DisabledDates), typeof(List<DateTime>), typeof(MonthDaysView), new List<DateTime>(), BindingMode.TwoWay, propertyChanged: DisabledDatesChanged);

    private static void DisabledDatesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MonthDaysView control && (newValue is List<DateTime> || newValue is null) && !Equals(newValue, oldValue))
            control.UpdateDays();
    }

    /// <summary>
    /// Selected date in single date selection mode
    /// </summary>
    public List<DateTime> DisabledDates
    {
        get => (List<DateTime>)GetValue(DisabledDatesProperty);
        set => SetValue(DisabledDatesProperty, value);
    }
    //////////////////////////////////////
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
    public static readonly BindableProperty SelectedDayViewBorderStyleProperty =
      BindableProperty.Create(nameof(SelectedDayViewBorderStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultSelectedDayViewBorderStyle);

    /// <summary>
    /// Background color of currently selected date
    /// </summary>
    public Style SelectedDayViewBorderStyle
    {
        get => (Style)GetValue(SelectedDayViewBorderStyleProperty);
        set => SetValue(SelectedDayViewBorderStyleProperty, value);
    }

    public static readonly BindableProperty DeselectedDayViewBorderStyleProperty =
      BindableProperty.Create(nameof(DeselectedDayViewBorderStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultDeselectedDayViewBorderStyle);

    /// <summary>
    /// Background color of currently Deselected date
    /// </summary>
    public Style DeselectedDayViewBorderStyle
    {
        get => (Style)GetValue(DeselectedDayViewBorderStyleProperty);
        set => SetValue(DeselectedDayViewBorderStyleProperty, value);
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
    public static readonly BindableProperty EventIndicatorStyleProperty =
      BindableProperty.Create(nameof(EventIndicatorStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultEventIndicatorStyle);

    /// <summary>
    /// Color of event indicator on dates
    /// </summary>
    public Style EventIndicatorStyle
    {
        get => (Style)GetValue(EventIndicatorStyleProperty);
        set => SetValue(EventIndicatorStyleProperty, value);
    }

    /// <summary>
    /// Bindable property for EventIndicatorSelectedStyle
    /// </summary>
    public static readonly BindableProperty EventIndicatorSelectedStyleProperty =
      BindableProperty.Create(nameof(EventIndicatorSelectedStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultEventIndicatorSelectedStyle);

    /// <summary>
    /// Color of event indicator on selected dates
    /// </summary>
    public Style EventIndicatorSelectedStyle
    {
        get => (Style)GetValue(EventIndicatorSelectedStyleProperty);
        set => SetValue(EventIndicatorSelectedStyleProperty, value);
    }


    /// <summary>
    /// Bindable property for TodayOutlineColor
    /// </summary>
    public static readonly BindableProperty TodayDayViewBorderStyleProperty =
      BindableProperty.Create(nameof(TodayDayViewBorderStyle), typeof(Style), typeof(Calendar), DefaultStyles.DefaultTodayDayViewBorderStyle);

    /// <summary>
    /// Specifies the color of outline for today's date
    /// </summary>
    public Style TodayDayViewBorderStyle
    {
        get => (Style)GetValue(TodayDayViewBorderStyleProperty);
        set => SetValue(TodayDayViewBorderStyleProperty, value);
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
      BindableProperty.Create(nameof(DayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style OtherMonthDaysLabelStyle
    {
        get => (Style)GetValue(OtherMonthDaysLabelStyleProperty);
        set => SetValue(OtherMonthDaysLabelStyleProperty, value);
    }

    public static readonly BindableProperty OtherMonthDaysLabelStyleProperty =
      BindableProperty.Create(nameof(OtherMonthDaysLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultOtherMonthDayLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style DeselectedDayLabelStyle
    {
        get => (Style)GetValue(DeselectedDayLabelStyleProperty);
        set => SetValue(DeselectedDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty DeselectedDayLabelStyleProperty =
      BindableProperty.Create(nameof(DeselectedDayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultDeselectedDayLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style WeekendDayLabelStyle
    {
        get => (Style)GetValue(WeekendDayLabelStyleProperty);
        set => SetValue(WeekendDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty WeekendDayLabelStyleProperty =
      BindableProperty.Create(nameof(WeekendDayLabelStyle), typeof(Style), typeof(MonthDaysView), null, propertyChanged: DayStylePropertyChanges);

    /// <summary>
    /// Bindable property for WeekendDaysPaintFirst
    /// </summary>
    public static readonly BindableProperty WeekendDaysPaintFirstProperty =
      BindableProperty.Create(nameof(WeekendDaysPaintFirst), typeof(bool), typeof(MonthDaysView), false, propertyChanged: DayStylePropertyChanges);

    /// <summary>
    /// Specifies whether the weekend days should be painted first
    /// </summary>
    public bool WeekendDaysPaintFirst
    {
        get => (bool)GetValue(WeekendDaysPaintFirstProperty);
        set => SetValue(WeekendDaysPaintFirstProperty, value);
    }

    public Style DisabledDayLabelStyle
    {
        get => (Style)GetValue(DisabledDayLabelStyleProperty);
        set => SetValue(DisabledDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty DisabledDayLabelStyleProperty =
      BindableProperty.Create(nameof(DisabledDayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultDisabledDayLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style SelectedTodayLabelStyle
    {
        get => (Style)GetValue(SelectedTodayLabelStyleProperty);
        set => SetValue(SelectedTodayLabelStyleProperty, value);
    }

    public static readonly BindableProperty SelectedTodayLabelStyleProperty =
      BindableProperty.Create(nameof(SelectedTodayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultSelectedTodayLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style SelectedDayLabelStyle
    {
        get => (Style)GetValue(SelectedDayLabelStyleProperty);
        set => SetValue(SelectedDayLabelStyleProperty, value);
    }

    public static readonly BindableProperty SelectedDayLabelStyleProperty =
      BindableProperty.Create(nameof(SelectedDayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultSelectedDayLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style TodayLabelStyle
    {
        get => (Style)GetValue(TodayLabelStyleProperty);
        set => SetValue(TodayLabelStyleProperty, value);
    }

    public static readonly BindableProperty TodayLabelStyleProperty =
      BindableProperty.Create(nameof(TodayLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultTodayLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style EventIndicatorSelectedLabelStyle
    {
        get => (Style)GetValue(EventIndicatorSelectedLabelStyleProperty);
        set => SetValue(EventIndicatorSelectedLabelStyleProperty, value);
    }

    public static readonly BindableProperty EventIndicatorSelectedLabelStyleProperty =
      BindableProperty.Create(nameof(EventIndicatorSelectedLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultEventIndicatorSelectedLabelStyle, propertyChanged: DayStylePropertyChanges);

    public Style EventIndicatorLabelStyle
    {
        get => (Style)GetValue(EventIndicatorLabelStyleProperty);
        set => SetValue(EventIndicatorLabelStyleProperty, value);
    }

    public static readonly BindableProperty EventIndicatorLabelStyleProperty =
      BindableProperty.Create(nameof(EventIndicatorLabelStyle), typeof(Style), typeof(MonthDaysView), DefaultStyles.DefaultEventIndicatorLabelStyle, propertyChanged: DayStylePropertyChanges);


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
      BindableProperty.Create(nameof(DaysTitleLabelStyle), typeof(Style), typeof(MonthDaysView), defaultValue: DefaultStyles.DefaultTitleDaysLabelStyle, propertyChanged: TitleStylePropertyChanges);


    /// <summary>
    /// ???
    /// </summary>
    public Style DaysTitleLabelStyle
    {
        get => (Style)GetValue(DaysTitleLabelStyleProperty);
        set => SetValue(DaysTitleLabelStyleProperty, value);
    }

    /// <summary>
    /// Bindable property for DaysTitleWeekendColor
    /// </summary>
    public static readonly BindableProperty DaysTitleWeekendStyleProperty =
      BindableProperty.Create(nameof(DaysTitleWeekendStyle), typeof(Style), typeof(MonthDaysView), null, propertyChanged: TitleStylePropertyChanges);

    /// <summary>
    /// Color of weekday titles
    /// </summary>
    public Style DaysTitleWeekendStyle
    {
        get => (Style)GetValue(DaysTitleWeekendStyleProperty);
        set => SetValue(DaysTitleWeekendStyleProperty, value);
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

    private static void TitleStylePropertyChanges(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MonthDaysView control && !Equals(newValue, oldValue))
            control.UpdateDayTitles();
    }

    private static void DayStylePropertyChanges(BindableObject bindable, object oldValue, object newValue)
    {
        //todo: separate to methods that update only styles
        if (bindable is MonthDaysView control && !Equals(newValue, oldValue))
            control.UpdateDays();
    }

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
            case nameof(SelectedDayViewBorderStyle):
            case nameof(DeselectedDayViewBorderStyle):
            case nameof(EventIndicatorStyle):
            case nameof(EventIndicatorSelectedStyle):
            case nameof(EventIndicatorType):
            case nameof(TodayDayViewBorderStyle):
                UpdateDaysColors();
                break;
            case nameof(Culture):
                RenderLayout();
                await UpdateAndAnimateDays(AnimateCalendar);
                break;

            case nameof(DaysTitleMaximumLength):
            case nameof(DaysTitleLabelStyle):
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
            var titleText = DaysTitleLabelFirstUpperRestLower ? abberivatedDayName[..1].ToUpperInvariant() + abberivatedDayName[1..].ToLowerInvariant() : abberivatedDayName.ToUpperInvariant();
            var calculatedTitleText = titleText[..((int)DaysTitleMaximumLength > abberivatedDayName.Length ? abberivatedDayName.Length : (int)DaysTitleMaximumLength)];
            if (dayLabel.Text != calculatedTitleText)
            {
                dayLabel.Text = calculatedTitleText;
            }

            if (!Equals(dayLabel.Style, DaysTitleLabelStyle))
            {
                dayLabel.Style = DaysTitleLabelStyle;
            }

            // Detect weekend days
            if (DaysTitleWeekendStyle != null && (dayNumber == (int)DayOfWeek.Saturday || dayNumber == (int)DayOfWeek.Sunday))
            {
                // It's a weekend day
                // You can change the color of the label or do something else
                if (!Equals(dayLabel.Style, DaysTitleWeekendStyle))
                {
                    dayLabel.Style = DaysTitleWeekendStyle;
                }
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
            dayModel.IsThisMonth = (CalendarLayout != WeekLayout.Month) || currentDate.Month == ShownDate.Month;
            dayModel.DaysLabelStyle = GetDayLabelStyle(dayModel, WeekendDaysPaintFirst);
            dayModel.OtherMonthIsVisible = (CalendarLayout != WeekLayout.Month) || OtherMonthDayIsVisible;
            dayModel.HasEvents = Events.ContainsKey(currentDate);
            dayModel.IsDisabled = currentDate < MinimumDate || currentDate > MaximumDate || DisabledDates.Contains(currentDate.Date);

            ChangePropertySilently(nameof(dayModel.IsSelected), () => dayModel.IsSelected = CurrentSelectionEngine.IsDateSelected(dayModel.Date));
            AssignIndicatorStyles(ref dayModel);
        }
    }

    Style GetDayLabelStyle(DayModel dayModel, bool weekendDaysPaintFirst)
    {
        return (dayModel.IsDisabled, dayModel.IsSelected, dayModel.HasEvents, dayModel.IsThisMonth, dayModel.IsToday, dayModel.IsWeekend, weekendDaysPaintFirst) switch
        {
            (true, _, _, _, _, _, _) => DisabledDayLabelStyle,
            (false, true, false, true, true, _, _) => SelectedTodayLabelStyle,
            (false, true, false, true, false, _, _) => SelectedDayLabelStyle,
            (false, false, false, true, true, _, _) => TodayLabelStyle,
            (false, _, _, _, _, true, true) => WeekendDayLabelStyle,
            (false, true, true, true, _, _, _) => EventIndicatorSelectedLabelStyle,
            (false, false, true, true, _, _, _) => EventIndicatorLabelStyle,
            (false, false, _, false, _, _, _) => OtherMonthDaysLabelStyle,
            (false, false, false, true, false, _, _) => DeselectedDayLabelStyle,
            (false, _, _, _, _, true, false) => WeekendDayLabelStyle,
            (_, _, _, _, _, _, _) => DayLabelStyle
        };
    }

    private void UpdateDaysColors()
    {
        foreach (var dayView in _dayViews)
        {
            var dayModel = dayView.BindingContext as DayModel;

            dayModel.SelectedDayViewBorderStyle = SelectedDayViewBorderStyle;
            dayModel.TodayDayViewBorderStyle = TodayDayViewBorderStyle;
            dayModel.DeselectedDayViewBorderStyle = DeselectedDayViewBorderStyle;

            AssignIndicatorStyles(ref dayModel);
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
            nameof(DaysTitleLabelStyle),
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

    internal void AssignIndicatorStyles(ref DayModel dayModel)
    {
        if (Events.TryGetValue(dayModel.Date, out var dayEventCollection) && dayEventCollection is IPersonalizableDayEvent personalizableDay)
        {
            dayModel.EventIndicatorStyle = personalizableDay?.EventIndicatorStyle ?? EventIndicatorStyle;
            dayModel.EventIndicatorSelectedStyle = personalizableDay?.EventIndicatorSelectedStyle ?? personalizableDay?.EventIndicatorStyle ?? EventIndicatorSelectedStyle;
            dayModel.EventIndicatorLabelStyle = personalizableDay?.EventIndicatorLabelStyle ?? EventIndicatorLabelStyle;
            dayModel.EventIndicatorSelectedLabelStyle = personalizableDay?.EventIndicatorSelectedLabelStyle ?? personalizableDay?.EventIndicatorLabelStyle ?? EventIndicatorSelectedLabelStyle;

        }
        else
        {
            dayModel.EventIndicatorStyle = EventIndicatorStyle;
            dayModel.EventIndicatorSelectedStyle = EventIndicatorSelectedStyle;
            dayModel.EventIndicatorLabelStyle = EventIndicatorLabelStyle;
            dayModel.EventIndicatorSelectedLabelStyle = EventIndicatorSelectedLabelStyle;
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
