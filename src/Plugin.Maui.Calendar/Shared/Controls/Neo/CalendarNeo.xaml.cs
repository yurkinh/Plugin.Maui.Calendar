using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class CalendarNeo : ContentView
{
    private readonly DateTime today = DateTime.Today;
    private readonly List<DayCell> dayCells = [];
    private readonly Dictionary<DayOfWeek, string> dayDictionary = weekNames;
    
    /// <summary>
    /// Calendar plugin for .NET MAUI
    /// </summary>
    public CalendarNeo()
    {
        PrevLayoutUnitCommand = new Command(PrevUnit);
        NextLayoutUnitCommand = new Command(NextUnit);
        PrevYearCommand = new Command(PrevYear);
        NextYearCommand = new Command(NextYear);

        InitializeComponent();
        CreateCalendarDayCells();
        InitDaysHeader();

    }

    #region Bindable properties

    /// <summary>
    /// Bindable property for ShowMonthPicker
    /// </summary>
    public static readonly BindableProperty ShowMonthPickerProperty = BindableProperty.Create(
        nameof(ShowMonthPicker),
        typeof(bool),
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
    /// Bindable property for Day
    /// </summary>
    public static readonly BindableProperty DayProperty = BindableProperty.Create(
        nameof(Day),
        typeof(int),
        typeof(CalendarNeo),
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

    /// <summary>
    /// Bindable property for Month
    /// </summary>
    public static readonly BindableProperty MonthProperty = BindableProperty.Create(
        nameof(Month),
        typeof(int),
        typeof(CalendarNeo),
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

    /// <summary>
    /// Bindable property for YearProperty
    /// </summary>
    public static readonly BindableProperty YearProperty = BindableProperty.Create(
        nameof(Year),
        typeof(int),
        typeof(CalendarNeo),
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

    /// <summary>
    /// Bindable property for InitalDate
    /// </summary>
    public static readonly BindableProperty ShownDateProperty = BindableProperty.Create(
        nameof(ShownDate),
        typeof(DateTime),
        typeof(CalendarNeo),
        DateTime.Today,
        BindingMode.TwoWay,
        propertyChanged: OnDateChanged
    );

    /// <summary>
    /// Specifies the Date that is initially shown
    /// </summary>
    public DateTime ShownDate
    {
        get => (DateTime)GetValue(ShownDateProperty);
        set => SetValue(ShownDateProperty, value);
    }

    /// <summary>
    /// Bindable property for InitalDate
    /// </summary>
    public static readonly BindableProperty OnShownDateChangedCommandProperty =
        BindableProperty.Create(
            nameof(OnShownDateChangedCommand),
            typeof(ICommand),
            typeof(CalendarNeo),
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
    /// Bindable property for Culture
    /// </summary>
    public static readonly BindableProperty CultureProperty = BindableProperty.Create(
        nameof(Culture),
        typeof(CultureInfo),
        typeof(CalendarNeo),
        CultureInfo.InvariantCulture,
        BindingMode.TwoWay
    );

    /// <summary>
    /// Specifies the culture to be used
    /// </summary>
    public CultureInfo Culture
    {
        get => (CultureInfo)GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    /// <summary>
    /// Bindable property for SelectedDayEvents
    /// </summary>
    public static readonly BindableProperty SelectedDayEventsProperty = BindableProperty.Create(
        nameof(SelectedDayEvents),
        typeof(ICollection),
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
    /// Bindable property for MonthLabelColor
    /// </summary>
    public static readonly BindableProperty MonthLabelColorProperty = BindableProperty.Create(
        nameof(MonthLabelColor),
        typeof(Color),
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
    /// Bindable property for DaysTitleColor
    /// </summary>
    public static readonly BindableProperty DaysTitleColorProperty = BindableProperty.Create(
        nameof(DaysTitleColor),
        typeof(Color),
        typeof(CalendarNeo),
        Colors.Black
    );

    /// <summary>
    /// Specifies the color for the titles of days
    /// </summary>
    public Color DaysTitleColor
    {
        get => (Color)GetValue(DaysTitleColorProperty);
        set => SetValue(DaysTitleColorProperty, value);
    }

    /// <summary>
    /// Bindable property for DaysTitleWeekendColor
    /// </summary>
    public static readonly BindableProperty DaysTitleWeekendColorProperty = BindableProperty.Create(
        nameof(DaysTitleWeekendColor),
        typeof(Color),
        typeof(CalendarNeo),
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

    #endregion

    #region Arrows
    /// <summary>
    /// Bindable property for ArrowsColor
    /// </summary>
    public static readonly BindableProperty ArrowsColorProperty = BindableProperty.Create(
        nameof(ArrowsColor),
        typeof(Color),
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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
        typeof(CalendarNeo),
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

    /// <summary>
    /// Bindable property for FooterArrowVisible
    /// </summary>
    public static readonly BindableProperty FooterArrowVisibleProperty = BindableProperty.Create(
        nameof(FooterArrowVisible),
        typeof(bool),
        typeof(CalendarNeo),
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

    /// <summary>
    /// Bindable property for HeaderSectionVisible
    /// </summary>
    public static readonly BindableProperty HeaderSectionVisibleProperty = BindableProperty.Create(
        nameof(HeaderSectionVisible),
        typeof(bool),
        typeof(CalendarNeo),
        true
    );

    #endregion

    #region Header & Footer
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
        typeof(CalendarNeo),
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

    /// <summary>
    /// Bindable property for HeaderSectionTemplate
    /// </summary>
    public static readonly BindableProperty HeaderSectionTemplateProperty = BindableProperty.Create(
        nameof(HeaderSectionTemplate),
        typeof(DataTemplate),
        typeof(CalendarNeo),
        new DataTemplate(() => new DefaultHeaderSectionNeo())
    );

    /// <summary>
    /// Data template for the header section
    /// </summary>
    public DataTemplate HeaderSectionTemplate
    {
        get => (DataTemplate)GetValue(HeaderSectionTemplateProperty);
        set => SetValue(HeaderSectionTemplateProperty, value);
    }

    /// <summary>
    /// Bindable property for FooterSectionTemplate
    /// </summary>
    public static readonly BindableProperty FooterSectionTemplateProperty = BindableProperty.Create(
        nameof(FooterSectionTemplate),
        typeof(DataTemplate),
        typeof(CalendarNeo),
        new DataTemplate(() => new DefaultFooterSectionNeo())
    );

    /// <summary>
    /// Data template for the footer section
    /// </summary>
    public DataTemplate FooterSectionTemplate
    {
        get => (DataTemplate)GetValue(FooterSectionTemplateProperty);
        set => SetValue(FooterSectionTemplateProperty, value);
    }

    /// <summary>
    /// Bindable property for MonthText
    /// </summary>
    public static readonly BindableProperty MonthTextProperty = BindableProperty.Create(
        nameof(LayoutUnitText),
        typeof(string),
        typeof(CalendarNeo),
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
    /// Bindable property for SelectedDateText
    /// </summary>
    public static readonly BindableProperty SelectedDateTextProperty = BindableProperty.Create(
        nameof(SelectedDateText),
        typeof(string),
        typeof(CalendarNeo),
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
    public static readonly BindableProperty SelectedDateTextFormatProperty =
        BindableProperty.Create(
            nameof(SelectedDateTextFormat),
            typeof(string),
            typeof(CalendarNeo),
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

    #region Events
    /// <summary>
    /// Bindable property for EventsScrollView
    /// </summary>
    public static readonly BindableProperty EventsScrollViewVisibleProperty =
        BindableProperty.Create(
            nameof(EventsScrollViewVisible),
            typeof(bool),
            typeof(CalendarNeo),
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
    #endregion

    #region DaysTitle
    /// <summary>
    /// Bindable property for DaysTitleLabelStyle
    /// </summary>
    public static readonly BindableProperty DaysTitleLabelStyleProperty = BindableProperty.Create(
        nameof(DaysTitleLabelStyle),
        typeof(Style),
        typeof(CalendarNeo),
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
    #endregion


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

    #endregion

    #region PropertyChanged



    #endregion


    #region Other methods

    private void PrevUnit()
    {

    }

    private void NextUnit()
    {

    }

    private void NextYear(object obj)
    {
        ShownDate = ShownDate.AddYears(1);
    }

    private void PrevYear(object obj)
    {
        ShownDate = ShownDate.AddYears(-1);
    }


    #endregion
}
