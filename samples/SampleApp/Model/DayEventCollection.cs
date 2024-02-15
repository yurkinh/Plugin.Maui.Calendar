﻿namespace SampleApp.Model;

/// <summary>
/// Wrapper to allow change the dot color
/// </summary>
public class DayEventCollection<T> : List<T>
{
	/// <summary>
	/// Empty contructor extends from base()
	/// </summary>
	public DayEventCollection() : base()
	{ }

	/// <summary>
	/// Color contructor extends from base()
	/// </summary>
	/// <param name="eventIndicatorStyle"></param>
	/// <param name="eventIndicatorSelectedStyle"></param>
	public DayEventCollection(Style eventIndicatorStyle, Style eventIndicatorSelectedStyle) : base()
	{
		EventIndicatorStyle = eventIndicatorStyle;
		EventIndicatorSelectedStyle = eventIndicatorSelectedStyle;
	}

	/// <summary>
	/// IEnumerable contructor extends from base(IEnumerable collection)
	/// </summary>
	/// <param name="collection"></param>
	public DayEventCollection(IEnumerable<T> collection) : base(collection)
	{ }

	/// <summary>
	/// Capacity contructor extends from base(int capacity)
	/// </summary>
	/// <param name="capacity"></param>
	public DayEventCollection(int capacity) : base(capacity)
	{ }

	#region PersonalizableProperties
	public Style EventIndicatorStyle { get; set; }
	public Style EventIndicatorSelectedStyle { get; set; }

	#endregion
}
