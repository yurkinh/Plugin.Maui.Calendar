using System.Collections;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Styles;
using Plugin.Maui.Calendar.Shared.Extensions;
using System.Collections.Specialized;
using System.Collections.ObjectModel;


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
}
