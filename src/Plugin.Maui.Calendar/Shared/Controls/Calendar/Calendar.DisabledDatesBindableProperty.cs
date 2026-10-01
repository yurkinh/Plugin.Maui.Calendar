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

	static void OnDisabledDatesChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.UpdateDays();
	}

	static void OnDisabledDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		var calendar = (Calendar)bindable;
		calendar.UpdateDaysColors();
	}
}
