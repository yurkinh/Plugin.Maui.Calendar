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
	/// Bindable property for DayViewBorderMargin
	/// </summary>
	public static readonly BindableProperty DayViewBorderMarginProperty = BindableProperty.Create(
		nameof(DayViewBorderMargin),
		typeof(Thickness),
		typeof(Calendar),
		new Thickness(8)
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
		DefaultStyles.DefaultLabelStyle,
		propertyChanged: OnDaysLabelStyleChanged
	);

	/// <summary>
	/// Specifies the style of day labels
	/// </summary>
	public Style DaysLabelStyle
	{
		get => (Style)GetValue(DaysLabelStyleProperty);
		set => SetValue(DaysLabelStyleProperty, value);
	}

	static void OnDaysLabelStyleChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDays(true);
		}
	}

	public static readonly BindableProperty EventIndicatorDotStyleProperty = BindableProperty.Create(
	nameof(EventIndicatorDotStyle),
	typeof(Style),
	typeof(Calendar),
	DefaultStyles.DefaultEventIndicatorDotStyle,
	propertyChanged: (b, o, n) => (b as Calendar)?.UpdateDays(true));

	public Style EventIndicatorDotStyle
	{
		get => (Style)GetValue(EventIndicatorDotStyleProperty);
		set => SetValue(EventIndicatorDotStyleProperty, value);
	}

	// EventIndicatorTextContainerStyle
	public static readonly BindableProperty EventIndicatorTextContainerStyleProperty = BindableProperty.Create(
		nameof(EventIndicatorTextContainerStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultEventIndicatorTextContainerStyle,
		propertyChanged: (b, o, n) => (b as Calendar)?.UpdateDays(true));

	public Style EventIndicatorTextContainerStyle
	{
		get => (Style)GetValue(EventIndicatorTextContainerStyleProperty);
		set => SetValue(EventIndicatorTextContainerStyleProperty, value);
	}

	// EventIndicatorTextStyle
	public static readonly BindableProperty EventIndicatorTextStyleProperty = BindableProperty.Create(
		nameof(EventIndicatorTextStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultEventIndicatorTextStyle,
		propertyChanged: (b, o, n) => (b as Calendar)?.UpdateDays(true));

	public Style EventIndicatorTextStyle
	{
		get => (Style)GetValue(EventIndicatorTextStyleProperty);
		set => SetValue(EventIndicatorTextStyleProperty, value);
	}

	// EventIndicatorImageStyle
	public static readonly BindableProperty EventIndicatorImageStyleProperty = BindableProperty.Create(
		nameof(EventIndicatorImageStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultEventIndicatorImageStyle,
		propertyChanged: (b, o, n) => (b as Calendar)?.UpdateDays(true));

	public Style EventIndicatorImageStyle
	{
		get => (Style)GetValue(EventIndicatorImageStyleProperty);
		set => SetValue(EventIndicatorImageStyleProperty, value);
	}
	
}
