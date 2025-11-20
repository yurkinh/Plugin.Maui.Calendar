namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	public static readonly BindableProperty DisabledDatesProperty = BindableProperty.Create(
		  nameof(DisabledDates),
		  typeof(List<DateTime>),
		  typeof(Calendar),
		  defaultValue: new List<DateTime>(),
		  BindingMode.TwoWay
	  );

	public List<DateTime> DisabledDates
	{
		get => (List<DateTime>)GetValue(DisabledDatesProperty);
		set => SetValue(DisabledDatesProperty, value);
	}

	/// <summary> Bindable property for DisabledDayColor </summary>
	public static readonly BindableProperty DisabledDayColorProperty = BindableProperty.Create(
		nameof(DisabledDayColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#ECECEC"),
		propertyChanged: OnDisabledDayColorChanged
	);

	/// <summary> Color for days which are out of MinimumDate - MaximumDate range </summary>
	public Color DisabledDayColor
	{
		get => (Color)GetValue(DisabledDayColorProperty);
		set => SetValue(DisabledDayColorProperty, value);
	}

	static void OnDisabledDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}
}
