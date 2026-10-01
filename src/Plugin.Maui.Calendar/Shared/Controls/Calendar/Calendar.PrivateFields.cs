using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Interfaces;


namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	// The swipe recognizers added while the calendar has a handler and swipe detection is on;
	// null otherwise.
	SwipeGestureRecognizer[] swipeGestures;

	const uint calendarSectionAnimationRate = 16;
	const int calendarSectionAnimationDuration = 200;
	const string calendarSectionAnimationId = nameof(calendarSectionAnimationId);
	bool calendarSectionAnimating;
	double calendarSectionHeight;
	IViewLayoutEngine CurrentViewLayoutEngine { get; set; }
	public ISelectionEngine CurrentSelectionEngine { get; set; } = new SingleSelectionEngine();
	protected readonly List<DayView> dayViews = [];

	// Item 13: cached references to the 7 day-of-week header labels populated in
	// RenderLayout so UpdateDayTitles can iterate them directly.
	Label[] dayTitleLabels = [];

	// Weekend-day background boxes, created lazily by UpdateWeekendBackground only while
	// WeekendDayBackgroundColor is set to a visible colour. Null when the feature is unused
	// (the default), so nothing extra is added to the visual tree.
	Border[] weekendBackgroundBands;

	// Whether the calendar has a handler. Set in OnHandlerChanging, where the Handler property
	// still holds the previous handler.
	bool isHandlerAttached;

	// Set when the handler is removed (DetachHandler disposes the calendar and stops observing
	// Events), so that AttachHandler knows the day cells may have missed Events changes.
	bool isHandlerDetached;
}
