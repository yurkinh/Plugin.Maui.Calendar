namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	public static readonly BindableProperty DisabledDatesProperty = BindableProperty.Create(
		  nameof(DisabledDates),
		  typeof(List<DateTime>),
		  typeof(Calendar),
		  defaultValue: null,
		  BindingMode.TwoWay,
		  propertyChanged: OnDisabledDatesChanged,
		  // Each calendar gets its own default list, so adding to one calendar's list never
		  // disables days in another calendar.
		  defaultValueCreator: static _ => new List<DateTime>()
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

	// The shown dates do not change, so the update is forced; otherwise UpdateDays returns early
	// and the visible cells keep their old IsDisabled state.
	static void OnDisabledDatesChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDays(forceUpdate: true);
		}
	}

	static void OnDisabledDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}
}
