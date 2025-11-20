namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
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
			Colors.White,
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
}
