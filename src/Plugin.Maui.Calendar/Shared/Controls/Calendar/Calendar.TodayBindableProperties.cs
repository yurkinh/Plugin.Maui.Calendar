namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
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
		var calendar = (Calendar)bindable;
		calendar.UpdateDaysColors();
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
		var calendar = (Calendar)bindable;
		calendar.UpdateDaysColors();
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
		var calendar = (Calendar)bindable;
		calendar.UpdateDaysColors();
	}

	/// <summary>
	/// Bindable property for TimeProvider
	/// </summary>
	public static readonly BindableProperty TimeProviderProperty = BindableProperty.Create(
		nameof(TimeProvider),
		typeof(TimeProvider),
		typeof(Calendar),
		TimeProvider.System,
		propertyChanged: OnTimeProviderChanged,
		coerceValue: static (bindable, value) => value ?? TimeProvider.System
	);

	/// <summary>
	/// Tells the calendar which day is today (the day drawn with <see cref="TodayOutlineColor"/>,
	/// <see cref="TodayFillColor"/> and <see cref="TodayTextColor"/>). <see cref="TimeProvider.System"/>
	/// (the default) follows the device clock; another provider can pin today to a fixed date, for
	/// example for tests and screenshots.
	/// </summary>
	public TimeProvider TimeProvider
	{
		get => (TimeProvider)GetValue(TimeProviderProperty);
		set => SetValue(TimeProviderProperty, value);
	}

	static void OnTimeProviderChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.RefreshToday();
		calendar.ScheduleTodayRefresh();
	}
}
