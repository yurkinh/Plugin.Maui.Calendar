namespace Plugin.Maui.Calendar.DV;

public partial class DelayedView : ALazyView
{
	public static readonly BindableProperty ViewProperty = BindableProperty.Create(
		nameof(View),
		typeof(View),
		typeof(DelayedView),
		default(View),
		propertyChanged: OnViewChanged);

	public View View
	{
		get => (View)GetValue(ViewProperty);
		set => SetValue(ViewProperty, value);
	}

	public int DelayInMilliseconds { get; set; } = 200;

	static async void OnViewChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is DelayedView control && newValue is View newView)
		{
			await Task.Delay(control.DelayInMilliseconds);
			if (!control.IsLoaded)
			{
				control.IsLoaded = true;
				control.Content = newView;
			}
		}
	}

	public override void LoadView()
	{
		if (IsLoaded)
		{
			return;
		}

		IsLoaded = true;
		Content = View;
	}
}
