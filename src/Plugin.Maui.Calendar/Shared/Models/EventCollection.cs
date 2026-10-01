using System.Collections;
using System.Collections.Specialized;

namespace Plugin.Maui.Calendar.Models;

/// <summary> 
/// Calendar events collection, wraps <see cref="Dictionary{DateTime, ICollection}" />.
/// </summary>
/// <remarks>
/// <para>
/// This class inherits <see cref="Dictionary{TKey,TValue}"/> and shadows the base
/// <c>Add</c>, <c>Remove</c>, <c>ContainsKey</c>, <c>TryGetValue</c> and the indexer
/// using the <c>new</c> keyword rather than overrides, because
/// <see cref="Dictionary{TKey,TValue}"/> does not declare those members as virtual.
/// Callers that hold a reference typed as <c>Dictionary&lt;DateTime, ICollection&gt;</c>
/// will bypass the change-notification logic.  Always use this type through its
/// own declared type (i.e. <see cref="EventCollection"/>) to guarantee notifications.
/// </para>
/// <para>
/// A day's collection that implements <see cref="INotifyCollectionChanged"/> (for example an
/// <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>) is observed too, so adding,
/// removing or replacing events inside it updates the calendar without assigning the day again.
/// </para>
/// </remarks>
public class EventCollection : Dictionary<DateTime, ICollection>
{
	// The observed day collections, each with the number of days that store it, so that a
	// collection stored for several days is subscribed to once and until its last day is removed.
	readonly Dictionary<INotifyCollectionChanged, int> observedDayCollections = new(ReferenceEqualityComparer.Instance);

	#region ctor

	/// <summary>
	/// Initializes a new instance of the <see cref="EventCollection"/> class
	/// that is empty, has the default initial capacity, and uses the default equality
	/// comparer for the key type.
	/// </summary>
	public EventCollection() : base()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="EventCollection"/> class
	/// that is empty, has the specified initial capacity, and uses the default equality
	/// comparer for the key type.
	/// </summary>
	/// <param name="capacity">
	/// The initial number of elements that the <see cref="EventCollection"/> can contain.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">capacity is less than 0.</exception>
	public EventCollection(int capacity) : base(capacity)
	{ }

	#endregion

	/// <summary>
	/// Removes a collection of values for specific date
	/// </summary>
	/// <param name="key">Event DateTime</param>
	/// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the System.Collections.Generic.Dictionary`2.</returns>
	public new bool Remove(DateTime key)
	{
		var removed = base.Remove(key.Date, out var dayEvents);

		if (removed)
		{
			StopObservingDayCollection(dayEvents);
			CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = key.Date, Type = EventCollectionChangedType.Remove });
		}

		return removed;
	}

	/// <summary>
	/// Add collection of values for specific date
	/// </summary>
	/// <param name="key">Event DateTime</param>
	/// <param name="value">Collection of events for date</param>
	public new void Add(DateTime key, ICollection value)
	{
		base.Add(key.Date, value);
		ObserveDayCollection(value);
		CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = key.Date, Type = EventCollectionChangedType.Add });
	}

	/// <summary>
	/// Gets/sets collection of values for specific date
	/// </summary>
	/// <param name="key">Event DateTime</param>
	/// <returns>Collection of events for date</returns>
	public new ICollection this[DateTime key]
	{
		get => base[key.Date];
		set
		{
			base.TryGetValue(key.Date, out var replaced);
			base[key.Date] = value;

			// Also right when the same collection is assigned again: it stays observed once.
			StopObservingDayCollection(replaced);
			ObserveDayCollection(value);

			CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = key.Date, Type = EventCollectionChangedType.Set });
		}
	}

	/// <summary>
	/// Checks if dictionary already has collection for specific date
	/// </summary>
	/// <param name="key">Key DateTime</param>
	/// <returns>true if dictionary already has the date as key; otherwise, false</returns>
	public new bool ContainsKey(DateTime key)
	{
		return base.ContainsKey(key.Date);
	}

	/// <summary>
	/// Gets the value associated with the specific date
	/// </summary>
	/// <param name="key">The date for the value to get</param>
	/// <param name="value">If the date exists then this is the associated collection; otherwise, it will be the default value of ICollection</param>
	/// <returns>true if dictionary contains an element with the specified date; otherwise false</returns>
	public new bool TryGetValue(DateTime key, out ICollection value)
	{
		return base.TryGetValue(key.Date, out value);
	}

	/// <summary>
	/// Gets the values associated with the specific date range
	/// </summary>
	/// <param name="keys"></param>
	/// <param name="values"></param>
	/// <returns></returns>
	public bool TryGetValues(ICollection<DateTime> keys, out ICollection values)
	{
		List<object> listToReturn = null;

		foreach (var currentDate in keys)
		{
			if (base.TryGetValue(currentDate, out var dayEvents))
			{
				listToReturn ??= [];
				foreach (var singleEvent in dayEvents)
				{
					listToReturn.Add(singleEvent);
				}
			}
		}

		if (listToReturn is not null && listToReturn.Count > 0)
		{
			values = listToReturn;
			return true;
		}
		else
		{
			values = new List<object>();
			return false;
		}
	}

	/// <summary>
	/// Removes all dates and collections
	/// </summary>
	public new void Clear()
	{
		foreach (var dayCollection in observedDayCollections.Keys)
		{
			dayCollection.CollectionChanged -= OnDayCollectionChanged;
		}
		observedDayCollections.Clear();

		if (base.Count == 0)
		{
			return;
		}

		base.Clear();
		CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = default, Type = EventCollectionChangedType.Clear });
	}

	void ObserveDayCollection(ICollection dayEvents)
	{
		if (dayEvents is not INotifyCollectionChanged dayCollection)
		{
			return;
		}

		if (observedDayCollections.TryGetValue(dayCollection, out var dayCount))
		{
			observedDayCollections[dayCollection] = dayCount + 1;
			return;
		}

		observedDayCollections[dayCollection] = 1;
		dayCollection.CollectionChanged += OnDayCollectionChanged;
	}

	void StopObservingDayCollection(ICollection dayEvents)
	{
		if (dayEvents is not INotifyCollectionChanged dayCollection
			|| !observedDayCollections.TryGetValue(dayCollection, out var dayCount))
		{
			return;
		}

		if (dayCount > 1)
		{
			observedDayCollections[dayCollection] = dayCount - 1;
			return;
		}

		observedDayCollections.Remove(dayCollection);
		dayCollection.CollectionChanged -= OnDayCollectionChanged;
	}

	// Item is left unset: the collection can be stored for more than one day.
	void OnDayCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) =>
		CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = default, Type = EventCollectionChangedType.DayCollectionChanged });

	internal event EventHandler<EventCollectionChangedArgs> CollectionChanged;

	internal sealed class EventCollectionChangedArgs
	{
		public DateTime Item { get; set; }
		public EventCollectionChangedType Type { get; set; }
	}

	internal enum EventCollectionChangedType
	{
		Add = 0,
		Set = 1,
		Remove = 2,
		Clear = 3,
		/// <summary>The events inside a day's own collection changed.</summary>
		DayCollectionChanged = 4
	}
}
