namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	static readonly BindablePropertyKey VisibleStartDatePropertyKey = BindableProperty.CreateReadOnly(
		nameof(VisibleStartDate),
		typeof(DateTime),
		typeof(Calendar),
		DateTime.Today,
		BindingMode.TwoWay
	);

	public static readonly BindableProperty VisibleStartDateProperty = VisibleStartDatePropertyKey.BindableProperty;

	/// <summary>
	/// The first date currently visible in the calendar view.
	/// </summary>
	public DateTime VisibleStartDate => (DateTime)GetValue(VisibleStartDateProperty);

	static readonly BindablePropertyKey VisibleEndDatePropertyKey = BindableProperty.CreateReadOnly(
		nameof(VisibleEndDate),
		typeof(DateTime),
		typeof(Calendar),
		DateTime.Today,
		BindingMode.TwoWay
	);

	public static readonly BindableProperty VisibleEndDateProperty = VisibleEndDatePropertyKey.BindableProperty;

	/// <summary>
	/// The last date currently visible in the calendar view.
	/// </summary>
	public DateTime VisibleEndDate => (DateTime)GetValue(VisibleEndDateProperty);
}
