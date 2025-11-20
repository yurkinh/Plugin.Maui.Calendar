namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
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
}
