using System.Collections;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for EventIndicatorType
	/// </summary>
	public static readonly BindableProperty EventIndicatorPlacementTypeProperty = BindableProperty.Create(
		nameof(EventIndicatorPlacementType),
		typeof(EventIndicatorPlacementType),
		typeof(Calendar),
		EventIndicatorPlacementType.Bottom,
		propertyChanged: OnEventIndicatorPlacementTypeChanged
	);

	/// <summary>
	/// Specifies the way in which events will be shown on dates
	/// </summary>
	public EventIndicatorPlacementType EventIndicatorPlacementType
	{
		get => (EventIndicatorPlacementType)GetValue(EventIndicatorPlacementTypeProperty);
		set => SetValue(EventIndicatorPlacementTypeProperty, value);
	}

	static void OnEventIndicatorPlacementTypeChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}

	public static readonly BindableProperty EventDayBackgroundColorIsActiveProperty = BindableProperty.Create(
			nameof(EventDayBackgroundColorIsActive),
			typeof(bool),
			typeof(Calendar),
			false
		);

	public bool EventDayBackgroundColorIsActive
	{
		get => (bool)GetValue(EventDayBackgroundColorIsActiveProperty);
		set => SetValue(EventDayBackgroundColorIsActiveProperty, value);
	}


	/// <summary>
	/// Bindable property for EventDayBackgroundColor
	/// </summary>
	public static readonly BindableProperty EventDayBackgroundColorProperty = BindableProperty.Create(
		nameof(EventDayBackgroundColor),
		typeof(Color),
		typeof(Calendar),
		Colors.DeepPink,
		propertyChanged: OnEventDayBackgroundColorChanged
	);

	/// <summary>
	/// Specifies the color of the event indicators
	/// </summary>
	public Color EventDayBackgroundColor
	{
		get => (Color)GetValue(EventDayBackgroundColorProperty);
		set => SetValue(EventDayBackgroundColorProperty, value);
	}

	static void OnEventDayBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for EventsScrollView
	/// </summary>
	public static readonly BindableProperty EventsScrollViewVisibleProperty = BindableProperty.Create(
			nameof(EventsScrollViewVisible),
			typeof(bool),
			typeof(Calendar),
			false
		);

	/// <summary>
	/// Specifies whether the events section is visible
	/// </summary>
	public bool EventsScrollViewVisible
	{
		get => (bool)GetValue(EventsScrollViewVisibleProperty);
		set => SetValue(EventsScrollViewVisibleProperty, value);
	}


	/// <summary>
	/// Bindable property for events
	/// </summary>
	public static readonly BindableProperty EventsProperty = BindableProperty.Create(
		nameof(Events),
		typeof(EventCollection),
		typeof(Calendar),
		new EventCollection(),
		propertyChanged: OnEventsChanged
	);

	/// <summary>
	/// Collection of all the events in the calendar
	/// </summary>
	public EventCollection Events
	{
		get => (EventCollection)GetValue(EventsProperty);
		set => SetValue(EventsProperty, value);
	}

	static void OnEventsChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			if (oldValue is EventCollection oldEvents)
			{
				oldEvents.CollectionChanged -= calendar.OnEventsCollectionChanged;
			}

			if (newValue is EventCollection newEvents)
			{
				newEvents.CollectionChanged += calendar.OnEventsCollectionChanged;
			}

			calendar.UpdateEvents();
			calendar.UpdateLayoutUnitLabel();
			//Todo: called two time at the calendar start
			calendar.UpdateDays(true);
		}
	}


	/// <summary>
	/// Bindable property for SelectedDayEvents
	/// </summary>
	public static readonly BindableProperty SelectedDayEventsProperty = BindableProperty.Create(
		nameof(SelectedDayEvents),
		typeof(ICollection),
		typeof(Calendar),
		new List<object>()
	);

	/// <summary>
	/// Collection of events on the selected date(s)
	/// </summary>
	public ICollection SelectedDayEvents
	{
		get => (ICollection)GetValue(SelectedDayEventsProperty);
		set => SetValue(SelectedDayEventsProperty, value);
	}

	/// <summary>
	/// Bindable property for EventTemplate
	/// </summary>
	public static readonly BindableProperty EventTemplateProperty = BindableProperty.Create(
		nameof(EventTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Specifies the template to be used for showing events
	/// </summary>
	public DataTemplate EventTemplate
	{
		get => (DataTemplate)GetValue(EventTemplateProperty);
		set => SetValue(EventTemplateProperty, value);
	}

	/// <summary>
	/// Bindable property for EmptyTemplate
	/// </summary>
	public static readonly BindableProperty EmptyTemplateProperty = BindableProperty.Create(
		nameof(EmptyTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Specifies the data template to be shown when there are no events
	/// </summary>
	public DataTemplate EmptyTemplate
	{
		get => (DataTemplate)GetValue(EmptyTemplateProperty);
		set => SetValue(EmptyTemplateProperty, value);
	}
}
