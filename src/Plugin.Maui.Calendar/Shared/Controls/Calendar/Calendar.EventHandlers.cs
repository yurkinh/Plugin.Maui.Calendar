using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{
		base.OnHandlerChanging(args);

		// Detach first: when one handler replaces another, detaching after attaching would undo
		// what AttachHandler just did (swipe recognizers, day-tap subscription).
		if (args.OldHandler is not null)
		{
			DetachHandler();
		}

		if (args.NewHandler is not null)
		{
			AttachHandler();
		}
	}

	void OnCalendarContainerSizeChanged(object sender, EventArgs e)
	{
		if (calendarContainer.Height > 0 && !calendarSectionAnimating)
		{
			UpdateCalendarSectionHeight();
		}
	}

	void OnSwipedRight(object sender, EventArgs e)
	{
		SwipeRightCommand?.Execute(null);

		if (SwipeToChangeMonthEnabled)
		{
			PrevUnit();
		}
	}

	void OnSwipedLeft(object sender, EventArgs e)
	{
		SwipeLeftCommand?.Execute(null);

		if (SwipeToChangeMonthEnabled)
		{
			NextUnit();
		}
	}

	void OnSwipedUp(object sender, EventArgs e)
	{
		SwipeUpCommand?.Execute(null);

		if (SwipeUpToHideEnabled)
		{
			ToggleCalendarSectionVisibility();
		}
	}

	void AttachHandler()
	{
		isHandlerAttached = true;
		calendarContainer.SizeChanged += OnCalendarContainerSizeChanged;
		WeakReferenceMessenger.Default.Register<Calendar, DayTappedMessage>(this, static (calendar, message) => calendar.OnDayTappedMessage(message));

		// DetachHandler disposes the calendar, which stops observing Events. A calendar that gets a
		// handler again (for example a cached page that is shown again) observes Events again and
		// catches up with the changes made while it had no handler.
		ObserveEvents();
		if (isHandlerDetached)
		{
			isHandlerDetached = false;
			UpdateEvents();
			UpdateDays();
		}

		UpdateSwipeGestures();
	}

	void DetachHandler()
	{
		isHandlerAttached = false;
		calendarContainer.SizeChanged -= OnCalendarContainerSizeChanged;
		WeakReferenceMessenger.Default.Unregister<DayTappedMessage>(this);
		RemoveSwipeGestures();

		//Todo remove later/when all event and properties will be refactored
		//all this should be done automaticall or not needed
		Dispose();
		isHandlerDetached = true;
	}

	/// <summary>
	/// Adds the swipe recognizers while the calendar has a handler and <see cref="SwipeDetectionDisabled"/>
	/// is off, and removes them otherwise.
	/// </summary>
	void UpdateSwipeGestures()
	{
		RemoveSwipeGestures();

		if (!isHandlerAttached || SwipeDetectionDisabled)
		{
			return;
		}

		swipeGestures =
		[
			new() { Direction = SwipeDirection.Left },
			new() { Direction = SwipeDirection.Right },
			new() { Direction = SwipeDirection.Up },
			new() { Direction = SwipeDirection.Down },
		];

		foreach (var swipeGesture in swipeGestures)
		{
			swipeGesture.Swiped += OnSwiped;
			GestureRecognizers.Add(swipeGesture);
		}
	}

	void RemoveSwipeGestures()
	{
		if (swipeGestures is null)
		{
			return;
		}

		foreach (var swipeGesture in swipeGestures)
		{
			swipeGesture.Swiped -= OnSwiped;
			GestureRecognizers.Remove(swipeGesture);
		}

		swipeGestures = null;
	}

	// Every calendar on screen receives every DayTappedMessage, so a tap is only handled by the
	// calendar that owns the tapped cell (a message without a source is handled by all, as before).
	void OnDayTappedMessage(DayTappedMessage message)
	{
		if (message.Source is Calendar owner && owner != this)
		{
			return;
		}

		OnDayTappedHandler(message.Value);
	}

	// Idempotent, so it can run whether or not Events is already observed.
	void ObserveEvents()
	{
		Events.CollectionChanged -= OnEventsCollectionChanged;
		Events.CollectionChanged += OnEventsCollectionChanged;
	}
}
