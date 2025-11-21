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
		default(Thickness)
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
}
