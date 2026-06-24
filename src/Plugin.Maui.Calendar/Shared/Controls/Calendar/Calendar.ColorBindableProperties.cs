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


	/// <summary>
	/// Bindable property for WeekendDayBackgroundColor
	/// </summary>
	public static readonly BindableProperty WeekendDayBackgroundColorProperty = BindableProperty.Create(
		nameof(WeekendDayBackgroundColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Transparent,
		propertyChanged: OnWeekendDayBackgroundChanged
	);

	/// <summary>
	/// Fills the background of each weekend (Saturday/Sunday) day cell. The day-of-week title
	/// row is left uncovered, and vertically-consecutive weekend days touch with no gap while
	/// remaining individual rounded boxes (see <see cref="WeekendDayBackgroundCornerRadius"/>).
	/// Defaults to <see cref="Colors.Transparent"/>, which keeps the calendar's original
	/// appearance so existing layouts are unaffected. The weekend columns are derived from
	/// <see cref="FirstDayOfWeek"/>.
	/// </summary>
	public Color WeekendDayBackgroundColor
	{
		get => (Color)GetValue(WeekendDayBackgroundColorProperty);
		set => SetValue(WeekendDayBackgroundColorProperty, value);
	}


	/// <summary>
	/// Bindable property for WeekendDayBackgroundCornerRadius
	/// </summary>
	public static readonly BindableProperty WeekendDayBackgroundCornerRadiusProperty = BindableProperty.Create(
		nameof(WeekendDayBackgroundCornerRadius),
		typeof(float),
		typeof(Calendar),
		0f,
		propertyChanged: OnWeekendDayBackgroundChanged
	);

	/// <summary>
	/// Corner radius applied to the <see cref="WeekendDayBackgroundColor"/> band. Defaults
	/// to <c>0</c> (square corners). Has no visual effect while the band is transparent.
	/// </summary>
	public float WeekendDayBackgroundCornerRadius
	{
		get => (float)GetValue(WeekendDayBackgroundCornerRadiusProperty);
		set => SetValue(WeekendDayBackgroundCornerRadiusProperty, value);
	}

	static void OnWeekendDayBackgroundChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateWeekendBackground();
		}
	}
}
