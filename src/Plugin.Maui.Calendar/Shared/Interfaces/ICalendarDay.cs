using System.ComponentModel;

namespace Plugin.Maui.Calendar.Interfaces;

/// <summary>
/// Read-only per-day data exposed to a <c>DayViewTemplate</c>. This is the binding context
/// of a custom day cell, allowing templates to react to a day's date and state.
/// </summary>
/// <remarks>
/// Day cells are reused: navigating to another month or week reassigns the same instances to
/// new dates. The calendar assigns the members whenever it updates its days, and every member
/// raises <see cref="INotifyPropertyChanged.PropertyChanged"/> when its value changes, so
/// templates should bind to these members rather than read them once.
/// </remarks>
public interface ICalendarDay : INotifyPropertyChanged
{
	/// <summary>The date this day cell represents.</summary>
	DateTime Date { get; }

	/// <summary>The formatted day-of-month text (e.g. "1".."31").</summary>
	string Day { get; }

	/// <summary>Whether this day is currently selected.</summary>
	/// <remarks>
	/// In a <c>RangeSelectionCalendar</c> this is <see langword="true"/> for every day inside the
	/// selected range, including its first and last day. Use <see cref="IsRangeStart"/> and
	/// <see cref="IsRangeEnd"/> to tell the range boundaries apart.
	/// </remarks>
	bool IsSelected { get; }

	/// <summary>Whether this day is today.</summary>
	/// <remarks>
	/// Kept up to date while the calendar is loaded, including when the local date changes at
	/// midnight.
	/// </remarks>
	bool IsToday { get; }

	/// <summary>Whether this day falls on a Saturday or a Sunday.</summary>
	/// <remarks>
	/// Depends only on <see cref="Date"/>; it does not depend on <c>WeekendDayColor</c> or
	/// <c>FirstDayOfWeek</c>.
	/// </remarks>
	bool IsWeekend { get; }

	/// <summary>Whether this day belongs to the month currently being displayed.</summary>
	/// <remarks>
	/// Always <see langword="true"/> in the <c>Week</c> and <c>TwoWeek</c> layouts, where every
	/// visible day counts as part of the shown period even when it crosses a month boundary.
	/// </remarks>
	bool IsThisMonth { get; }

	/// <summary>Whether this day is disabled (out of range or explicitly disabled).</summary>
	bool IsDisabled { get; }

	/// <summary>Whether this day has an entry in the calendar's <c>Events</c> collection.</summary>
	bool HasEvents { get; }

	/// <summary>
	/// The number of events in this day's entry of the calendar's <c>Events</c> collection, or
	/// 0 when the day has no entry.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A day whose entry is an empty collection has <see cref="HasEvents"/> set to
	/// <see langword="true"/> and an <see cref="EventCount"/> of 0.
	/// </para>
	/// <para>
	/// The count is read when the calendar updates its days, which happens when entries of the
	/// <c>Events</c> collection are added, replaced or removed. Adding to or removing from the
	/// collection already stored for a date is not observed; assign the entry again
	/// (<c>Events[date] = events</c>) to refresh the count.
	/// </para>
	/// </remarks>
	int EventCount { get; }

	/// <summary>
	/// The events stored for this day in the calendar's <c>Events</c> collection, in their original
	/// order, or an empty list when the day has none.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The items are the objects you added (for example your own event model), so a template can
	/// show them directly, e.g. <c>BindableLayout.ItemsSource="{Binding Events}"</c> to write each
	/// event's text inside the day cell.
	/// </para>
	/// <para>
	/// The list is a snapshot taken when the calendar updates its days and is replaced only when
	/// its items change. Like <see cref="EventCount"/>, adding to or removing from the collection
	/// already stored for a date is not observed; assign the entry again
	/// (<c>Events[date] = events</c>) to refresh it.
	/// </para>
	/// </remarks>
	IReadOnlyList<object> Events { get; }

	/// <summary>The colors of the event indicators for this day.</summary>
	IReadOnlyList<Color> EventColors { get; }

	/// <summary>Whether this day is the first day of the selected range.</summary>
	/// <remarks>
	/// Only set by <c>RangeSelectionCalendar</c>; always <see langword="false"/> for other
	/// calendars. When the range covers a single day (for example right after the first tap),
	/// both <see cref="IsRangeStart"/> and <see cref="IsRangeEnd"/> are <see langword="true"/>.
	/// </remarks>
	bool IsRangeStart { get; }

	/// <summary>Whether this day is the last day of the selected range.</summary>
	/// <remarks>
	/// Only set by <c>RangeSelectionCalendar</c>; always <see langword="false"/> for other
	/// calendars. When the range covers a single day (for example right after the first tap),
	/// both <see cref="IsRangeStart"/> and <see cref="IsRangeEnd"/> are <see langword="true"/>.
	/// </remarks>
	bool IsRangeEnd { get; }
}
