using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;


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

	/// <summary>
	/// No longer populated: days are drawn by DrawnUI instead of one native <see cref="DayView"/> each.
	/// Kept (always empty) so the public API stays binary compatible.
	/// </summary>
	[Obsolete("Days are now drawn; DayView instances are no longer created and this list is always empty.")]
	protected readonly List<DayView> dayViews = [];

	// One model per drawn day cell, in display order (replaces dayViews internally)
	private protected readonly List<DayModel> dayModels = [];

	DaysGrid daysGrid;

	// Resolve the public Label styles for the drawn labels (see LabelStyleBridge)
	LabelStyleBridge daysLabelStyleBridge;
	LabelStyleBridge daysTitleStyleBridge;
	LabelStyleBridge weekendTitleStyleBridge;

	// A swipe over the days can be seen both by the canvas and by the native
	// SwipeGestureRecognizers (platform dependent); handle it once.
	static readonly TimeSpan swipeDedupWindow = TimeSpan.FromMilliseconds(400);
	DateTime lastSwipeTime = DateTime.MinValue;
	SwipeDirection lastSwipeDirection;

	// Item 16: guard flag set during construction so that bindable-property callbacks
	// that fire before the control is fully initialised skip expensive render passes.
	// A single consolidated render executes at the end of the constructor.
	bool isInitializing;
}
