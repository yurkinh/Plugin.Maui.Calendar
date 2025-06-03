namespace Plugin.Maui.Calendar.Controls;

public partial class DelayedView : LazyView
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

	static async Task OnViewChanged(BindableObject bindable, object oldValue, object newValue)
	{
		try
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
		catch (Exception ex)
		{
			// Log or handle the exception as needed
			Console.WriteLine($"Error in OnViewChanged: {ex}");
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
