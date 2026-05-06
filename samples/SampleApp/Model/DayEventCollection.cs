using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;

namespace SampleApp.Model;

/// <summary>
/// Wrapper to allow change the dot color
/// </summary>
public class DayEventCollection<T> : List<T>, IPersonalizableDayEvent, IMultiEventDay
{
    /// <summary>
    /// Empty contructor extends from base()
    /// </summary>
    public DayEventCollection() : base()
    { }


    public DayEventCollection(EventIndicatorModel eventIndicator) : base()
    {
		EventIndicator = eventIndicator;
    }

    /// <summary>
    /// IEnumerable contructor extends from base(IEnumerable collection)
    /// </summary>
    /// <param name="collection"></param>
    public DayEventCollection(IEnumerable<T> collection) : base(collection)
    { 
    }

    /// <summary>
    /// Capacity contructor extends from base(int capacity)
    /// </summary>
    /// <param name="capacity"></param>
    public DayEventCollection(int capacity) : base(capacity)
    { }

	#region PersonalizableProperties
	public EventIndicatorModel EventIndicator { get; set; }


	#endregion

	#region IMultiEventDay
	public IReadOnlyList<EventIndicatorModel> EventIndicators { get; set; }
	#endregion
}
