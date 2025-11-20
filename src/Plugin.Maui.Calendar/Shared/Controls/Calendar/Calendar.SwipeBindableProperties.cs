using System.Windows.Input;

namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for DisableSwipeDetection
	/// </summary>
	public static readonly BindableProperty SwipeDetectionDisabledProperty = BindableProperty.Create(
			 nameof(SwipeDetectionDisabled),
			 typeof(bool),
			 typeof(Calendar),
			 false
		 );

	public bool SwipeDetectionDisabled
	{
		get => (bool)GetValue(SwipeDetectionDisabledProperty);
		set => SetValue(SwipeDetectionDisabledProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeUpCommand
	/// </summary>
	public static readonly BindableProperty SwipeUpCommandProperty = BindableProperty.Create(
		nameof(SwipeUpCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Activated when user swipes-up over days view
	/// </summary>
	public ICommand SwipeUpCommand
	{
		get => (ICommand)GetValue(SwipeUpCommandProperty);
		set => SetValue(SwipeUpCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeUpToHideEnabled
	/// </summary>
	public static readonly BindableProperty SwipeUpToHideEnabledProperty = BindableProperty.Create(
		nameof(SwipeUpToHideEnabled),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Enable/disable default swipe-up action for showing/hiding calendar
	/// </summary>
	public bool SwipeUpToHideEnabled
	{
		get => (bool)GetValue(SwipeUpToHideEnabledProperty);
		set => SetValue(SwipeUpToHideEnabledProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeLeftCommand
	/// </summary>
	public static readonly BindableProperty SwipeLeftCommandProperty = BindableProperty.Create(
		nameof(SwipeLeftCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Activated when user swipes-left over days view
	/// </summary>
	public ICommand SwipeLeftCommand
	{
		get => (ICommand)GetValue(SwipeLeftCommandProperty);
		set => SetValue(SwipeLeftCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeRightCommand
	/// </summary>
	public static readonly BindableProperty SwipeRightCommandProperty = BindableProperty.Create(
		nameof(SwipeRightCommand),
		typeof(ICommand),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Activated when user swipes-right over days view
	/// </summary>
	public ICommand SwipeRightCommand
	{
		get => (ICommand)GetValue(SwipeRightCommandProperty);
		set => SetValue(SwipeRightCommandProperty, value);
	}

	/// <summary>
	/// Bindable property for SwipeToChangeMonthEnabled
	/// </summary>
	public static readonly BindableProperty SwipeToChangeMonthEnabledProperty = BindableProperty.Create(
			nameof(SwipeToChangeMonthEnabled),
			typeof(bool),
			typeof(Calendar),
			true
		);

	/// <summary>
	/// Enable/disable default swipe actions for changing months
	/// </summary>
	public bool SwipeToChangeMonthEnabled
	{
		get => (bool)GetValue(SwipeToChangeMonthEnabledProperty);
		set => SetValue(SwipeToChangeMonthEnabledProperty, value);
	}
}
