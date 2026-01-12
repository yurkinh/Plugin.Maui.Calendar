using System.Collections;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for EventIndicatorType
	/// </summary>
	public static readonly BindableProperty EventIndicatorTypeProperty = BindableProperty.Create(
		nameof(EventIndicatorType),
		typeof(EventIndicatorType),
		typeof(Calendar),
		EventIndicatorType.BottomDot,
		propertyChanged: OnEventIndicatorTypeChanged
	);

	/// <summary>
	/// Specifies the way in which events will be shown on dates
	/// </summary>
	public EventIndicatorType EventIndicatorType
	{
		get => (EventIndicatorType)GetValue(EventIndicatorTypeProperty);
		set => SetValue(EventIndicatorTypeProperty, value);
	}

	static void OnEventIndicatorTypeChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
			calendar.UpdateDaysEventIndicatorColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorColor
	/// </summary>
	public static readonly BindableProperty EventIndicatorColorProperty = BindableProperty.Create(
		nameof(EventIndicatorColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#FF4081"),
		propertyChanged: OnEventIndicatorColorChanged
	);

	/// <summary>
	/// Specifies the color of the event indicators
	/// </summary>
	public Color EventIndicatorColor
	{
		get => (Color)GetValue(EventIndicatorColorProperty);
		set => SetValue(EventIndicatorColorProperty, value);
	}

	static void OnEventIndicatorColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
			calendar.UpdateDaysEventIndicatorColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorSelectedColor
	/// </summary>
	public static readonly BindableProperty EventIndicatorSelectedColorProperty = BindableProperty.Create(
			nameof(EventIndicatorSelectedColor),
			typeof(Color),
			typeof(Calendar),
			Color.FromArgb("#FF4081"),
			propertyChanged: OnEventIndicatorSelectedColorChanged
		);

	/// <summary>
	/// Specifies the color of the event indicators on selected dates
	/// </summary>
	public Color EventIndicatorSelectedColor
	{
		get => (Color)GetValue(EventIndicatorSelectedColorProperty);
		set => SetValue(EventIndicatorSelectedColorProperty, value);
	}

	static void OnEventIndicatorSelectedColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
			calendar.UpdateDaysEventIndicatorColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorTextColor
	/// </summary>
	public static readonly BindableProperty EventIndicatorTextColorProperty = BindableProperty.Create(
			nameof(EventIndicatorTextColor),
			typeof(Color),
			typeof(Calendar),
			Colors.Black,
			propertyChanged: OnEventIndicatorTextColorChanged
		);

	/// <summary>
	/// Specifies the color of the event indicator text
	/// </summary>
	public Color EventIndicatorTextColor
	{
		get => (Color)GetValue(EventIndicatorTextColorProperty);
		set => SetValue(EventIndicatorTextColorProperty, value);
	}

	static void OnEventIndicatorTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
			calendar.UpdateDaysEventIndicatorColors();
		}
	}


	/// <summary>
	/// Bindable property for EventIndicatorSelectedText
	/// </summary>
	public static readonly BindableProperty EventIndicatorSelectedTextColorProperty = BindableProperty.Create(
			nameof(EventIndicatorSelectedTextColor),
			typeof(Color),
			typeof(Calendar),
			Colors.Black,
			propertyChanged: OnEventIndicatorSelectedTextColorChanged
		);

	/// <summary>
	/// Specifies the color of the event indicator text on selected dates
	/// </summary>
	public Color EventIndicatorSelectedTextColor
	{
		get => (Color)GetValue(EventIndicatorSelectedTextColorProperty);
		set => SetValue(EventIndicatorSelectedTextColorProperty, value);
	}

	static void OnEventIndicatorSelectedTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
			calendar.UpdateDaysEventIndicatorColors();
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
