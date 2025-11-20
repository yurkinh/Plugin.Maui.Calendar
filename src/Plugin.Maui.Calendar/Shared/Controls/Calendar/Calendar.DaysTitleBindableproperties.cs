using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Styles;


namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for DaysTitleLabelStyle 
	/// </summary>
	public static readonly BindableProperty DaysTitleLabelStyleProperty = BindableProperty.Create(
		nameof(DaysTitleLabelStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultDaysTitleLabelStyle
	);

	/// <summary>
	/// Specifies the style of weekday title labels
	/// </summary>
	public Style DaysTitleLabelStyle
	{
		get => (Style)GetValue(DaysTitleLabelStyleProperty);
		set => SetValue(DaysTitleLabelStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for WeekendTitleStyle
	/// </summary>
	public static readonly BindableProperty WeekendTitleStyleProperty = BindableProperty.Create(
		nameof(WeekendTitleStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultWeekendTitleStyle,
		propertyChanged: OnWeekendTitleStyleChanged
	);

	/// <summary>
	/// Specifies the style of weekend title labels
	/// </summary>
	public Style WeekendTitleStyle
	{
		get => (Style)GetValue(WeekendTitleStyleProperty);
		set => SetValue(WeekendTitleStyleProperty, value);
	}

	static void OnWeekendTitleStyleChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}

	public static readonly BindableProperty UseAbbreviatedDayNamesProperty =
	BindableProperty.Create(
		nameof(UseAbbreviatedDayNames),
		typeof(bool),
		typeof(Calendar),
		false,
		propertyChanged: OnUseAbbreviatedDayNamesChanged
		);

	public bool UseAbbreviatedDayNames
	{
		get => (bool)GetValue(UseAbbreviatedDayNamesProperty);
		set => SetValue(UseAbbreviatedDayNamesProperty, value);
	}

	static void OnUseAbbreviatedDayNamesChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}

	/// <summary>
	/// Bindable property for DaysTitleMaximumLength
	/// </summary>
	public static readonly BindableProperty DaysTitleMaximumLengthProperty = BindableProperty.Create(
			nameof(DaysTitleMaximumLength),
			typeof(DaysTitleMaxLength),
			typeof(Calendar),
			DaysTitleMaxLength.ThreeChars,
			propertyChanged: OnDaysTitleMaximumLengthChanged
		);

	/// <summary>
	/// Specifies the maximum length of weekday titles
	/// </summary>
	public DaysTitleMaxLength DaysTitleMaximumLength
	{
		get => (DaysTitleMaxLength)GetValue(DaysTitleMaximumLengthProperty);
		set => SetValue(DaysTitleMaximumLengthProperty, value);
	}

	static void OnDaysTitleMaximumLengthChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}

	/// <summary>
	/// Bindable property for DaysTitleLabelFirstUpperRestLower
	/// </summary>
	public static readonly BindableProperty DaysTitleLabelFirstUpperRestLowerProperty = BindableProperty.Create(
			nameof(DaysTitleLabelFirstUpperRestLower),
			typeof(bool),
			typeof(Calendar),
			false,
			propertyChanged: OnDaysTitleLabelFirstUpperRestLowerChanged
		);

	/// <summary>
	/// Makes DaysTitleLabel text FirstCase Upper and rest lower
	/// </summary>
	public bool DaysTitleLabelFirstUpperRestLower
	{
		get => (bool)GetValue(DaysTitleLabelFirstUpperRestLowerProperty);
		set => SetValue(DaysTitleLabelFirstUpperRestLowerProperty, value);
	}

	static void OnDaysTitleLabelFirstUpperRestLowerChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDayTitles();
		}
	}
}
