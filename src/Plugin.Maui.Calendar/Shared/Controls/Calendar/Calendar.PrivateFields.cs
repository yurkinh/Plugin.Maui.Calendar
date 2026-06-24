using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Interfaces;


namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	SwipeGestureRecognizer leftSwipeGesture;
	SwipeGestureRecognizer rightSwipeGesture;
	SwipeGestureRecognizer upSwipeGesture;
	SwipeGestureRecognizer downSwipeGesture;

	const uint calendarSectionAnimationRate = 16;
	const int calendarSectionAnimationDuration = 200;
	const string calendarSectionAnimationId = nameof(calendarSectionAnimationId);
	readonly Lazy<Animation> calendarSectionAnimateHide;
	readonly Lazy<Animation> calendarSectionAnimateShow;
	bool calendarSectionAnimating;
	double calendarSectionHeight;
	IViewLayoutEngine CurrentViewLayoutEngine { get; set; }
	public ISelectionEngine CurrentSelectionEngine { get; set; } = new SingleSelectionEngine();
	protected readonly List<DayView> dayViews = [];

	// Item 13: cached references to the 7 day-of-week header labels populated in
	// RenderLayout so UpdateDayTitles can iterate them directly.
	Label[] dayTitleLabels;

	// Weekend-day background boxes, created lazily by UpdateWeekendBackground only while
	// WeekendDayBackgroundColor is set to a visible colour. Null when the feature is unused
	// (the default), so nothing extra is added to the visual tree.
	Border[] weekendBackgroundBands;

	// Item 16: guard flag set during construction so that bindable-property callbacks
	// that fire before the control is fully initialised skip expensive render passes.
	// A single consolidated render executes at the end of the constructor.
	bool isInitializing;
}
