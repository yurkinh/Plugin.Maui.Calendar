using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Styles;


namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for DayViewSize
	/// </summary>
	public static readonly BindableProperty DayViewSizeProperty = BindableProperty.Create(
		nameof(DayViewSize),
		typeof(double),
		typeof(Calendar),
		40.0,
		propertyChanged: OnDayViewGlobalPropertyChanged
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
	/// Bindable property for DayViewBorderMargin
	/// </summary>
	public static readonly BindableProperty DayViewBorderMarginProperty = BindableProperty.Create(
		nameof(DayViewBorderMargin),
		typeof(Thickness),
		typeof(Calendar),
		default(Thickness),
		propertyChanged: OnDayViewGlobalPropertyChanged
	);

	/// <summary>
	/// Specifies the margin of dayview border
	/// </summary>
	public Thickness DayViewBorderMargin
	{
		get => (Thickness)GetValue(DayViewBorderMarginProperty);
		set => SetValue(DayViewBorderMarginProperty, value);
	}

	/// <summary>
	/// Bindable property for DayViewCornerRadius
	/// </summary>
	public static readonly BindableProperty DayViewCornerRadiusProperty = BindableProperty.Create(
		nameof(DayViewCornerRadius),
		typeof(float),
		typeof(Calendar),
		20f,
		propertyChanged: OnDayViewGlobalPropertyChanged
	);

	/// <summary>
	/// Specifies the corner radius of individual dates
	/// </summary>
	public float DayViewCornerRadius
	{
		get => (float)GetValue(DayViewCornerRadiusProperty);
		set => SetValue(DayViewCornerRadiusProperty, value);
	}

	/// <summary>
	/// Bindable property for DaysLabelStyle
	/// </summary>
	public static readonly BindableProperty DaysLabelStyleProperty = BindableProperty.Create(
		nameof(DaysLabelStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultLabelStyle,
		propertyChanged: OnDayViewGlobalPropertyChanged
	);

	/// <summary>
	/// Specifies the style of day labels
	/// </summary>
	public Style DaysLabelStyle
	{
		get => (Style)GetValue(DaysLabelStyleProperty);
		set => SetValue(DaysLabelStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for DayViewTemplate
	/// </summary>
	public static readonly BindableProperty DayViewTemplateProperty = BindableProperty.Create(
		nameof(DayViewTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		null,
		propertyChanged: OnDayViewTemplateChanged
	);

	/// <summary>
	/// Specifies a custom template for drawing each day cell. When <see langword="null"/> (the
	/// default), the built-in day cell is used; setting it back to <see langword="null"/> restores it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The binding context of the template is an <see cref="ICalendarDay"/>. Day cells
	/// are reused when the calendar navigates, so bind to its members instead of reading them once.
	/// The root of the template must be a <see cref="View"/>, such as a <see cref="Grid"/>,
	/// <see cref="Border"/> or <see cref="Label"/>; any other root (for example a
	/// <see cref="ViewCell"/>) throws an <see cref="InvalidOperationException"/> when the cell is
	/// created. The template can be changed at any time; only the content of each cell is replaced.
	/// </para>
	/// <para>
	/// The template replaces what is drawn inside the cell. The calendar still sizes every cell to
	/// <see cref="DayViewSize"/>, hides the days that <see cref="OtherMonthDayIsVisible"/> and
	/// <see cref="OtherMonthWeekIsVisible"/> hide, and selects a day when its cell is tapped.
	/// The properties that style the built-in cell are not applied to a templated cell:
	/// <see cref="DaysLabelStyle"/>, <see cref="DayViewCornerRadius"/>,
	/// <see cref="DayViewBorderMargin"/>, <see cref="EventIndicatorType"/> (including the
	/// <c>BackgroundFull</c> cell background) and the day text, background and outline colors,
	/// such as <see cref="SelectedDayBackgroundColor"/>, <see cref="SelectedDayTextColor"/>,
	/// <see cref="DeselectedDayTextColor"/>, <see cref="TodayOutlineColor"/>,
	/// <see cref="TodayFillColor"/>, <see cref="WeekendDayColor"/>, <see cref="OtherMonthDayColor"/>,
	/// <see cref="DisabledDayColor"/> and <see cref="EventIndicatorTextColor"/>. Draw these states
	/// from the <see cref="ICalendarDay"/> members instead. <see cref="EventIndicatorColor"/>
	/// and <see cref="EventIndicatorSelectedColor"/> still provide
	/// <see cref="ICalendarDay.EventColors"/> for days whose events do not set their own
	/// colors, and <see cref="WeekendDayBackgroundColor"/> is still drawn behind the cells.
	/// </para>
	/// <para>
	/// A <see cref="DataTemplateSelector"/> is supported. It receives the
	/// <see cref="ICalendarDay"/> as the item and the cell as the container. It is asked
	/// when a cell is created and again for every cell each time the calendar updates its days: after
	/// navigating, and when the selection, the events, the disabled dates, the minimum or maximum
	/// date or today (at midnight) change. It always sees the complete, current state of the day, so
	/// it can choose by any <see cref="ICalendarDay"/> member. A cell only replaces its
	/// content when the selector returns a different template instance, so return the same instances
	/// every time (for example templates stored in properties of the selector) and keep the selector
	/// cheap. A selector that returns <see langword="null"/> shows the built-in cell for that day.
	/// </para>
	/// <para>
	/// Taps are handled by the cell around the template. A child that handles input itself, such as
	/// a <see cref="Button"/>, a <see cref="CheckBox"/> or a view with its own gesture recognizers,
	/// takes the tap and the day is not selected. Set <c>InputTransparent="True"</c> on such a child
	/// if tapping it should select the day.
	/// </para>
	/// </remarks>
	public DataTemplate DayViewTemplate
	{
		get => (DataTemplate)GetValue(DayViewTemplateProperty);
		set => SetValue(DayViewTemplateProperty, value);
	}

	// Only the content inside each cell changes: the grid, the day models and their state are kept,
	// so neither a layout rebuild nor an UpdateDays pass is needed. This also means a template set
	// in XAML, which is assigned after the constructor has built the grid, does not rebuild it.
	// Cells that were not rendered yet create their content later, directly from the new template.
	static void OnDayViewTemplateChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			foreach (var dayView in calendar.dayViews)
			{
				dayView.SetDayViewTemplate((DataTemplate)newValue);
			}
		}
	}

	// Item 2: a single handler for all structural DayView properties so that changes
	// only trigger UpdateDayGlobalProperties (one pass) instead of UpdateDays (full
	// date-recomputation pass).
	static void OnDayViewGlobalPropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayGlobalProperties();
		}
	}
}
