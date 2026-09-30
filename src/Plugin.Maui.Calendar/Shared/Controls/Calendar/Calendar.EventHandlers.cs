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
		if (args.OldHandler != null)
		{
			DetachHandler();
		}

		if (args.NewHandler != null)
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
			UpdateDays(forceUpdate: true);
		}

		if (!SwipeDetectionDisabled)
		{
			AddSwipeGestures();
		}
	}

	void DetachHandler()
	{
		calendarContainer.SizeChanged -= OnCalendarContainerSizeChanged;
		WeakReferenceMessenger.Default.Unregister<DayTappedMessage>(this);

		// Removes what AttachHandler added, whatever SwipeDetectionDisabled is now.
		RemoveSwipeGestures();
		//Todo remove later/when all event and properties will be refactored
		//all this should be done automaticall or not needed
		Dispose();
		isHandlerDetached = true;
	}

	// Idempotent, so SwipeDetectionDisabled can be switched at any time.
	void AddSwipeGestures()
	{
		if (leftSwipeGesture is not null)
		{
			return;
		}

		leftSwipeGesture = new() { Direction = SwipeDirection.Left };
		rightSwipeGesture = new() { Direction = SwipeDirection.Right };
		upSwipeGesture = new() { Direction = SwipeDirection.Up };
		downSwipeGesture = new() { Direction = SwipeDirection.Down };

		leftSwipeGesture.Swiped += OnSwiped;
		rightSwipeGesture.Swiped += OnSwiped;
		upSwipeGesture.Swiped += OnSwiped;
		downSwipeGesture.Swiped += OnSwiped;

		GestureRecognizers.Add(leftSwipeGesture);
		GestureRecognizers.Add(rightSwipeGesture);
		GestureRecognizers.Add(upSwipeGesture);
		GestureRecognizers.Add(downSwipeGesture);
	}

	// Removes only the calendar's own recognizers; ones the app added to the calendar stay.
	void RemoveSwipeGestures()
	{
		if (leftSwipeGesture is null)
		{
			return;
		}

		leftSwipeGesture.Swiped -= OnSwiped;
		rightSwipeGesture.Swiped -= OnSwiped;
		upSwipeGesture.Swiped -= OnSwiped;
		downSwipeGesture.Swiped -= OnSwiped;

		GestureRecognizers.Remove(leftSwipeGesture);
		GestureRecognizers.Remove(rightSwipeGesture);
		GestureRecognizers.Remove(upSwipeGesture);
		GestureRecognizers.Remove(downSwipeGesture);

		leftSwipeGesture = null;
		rightSwipeGesture = null;
		upSwipeGesture = null;
		downSwipeGesture = null;
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
		if (Events is EventCollection events)
		{
			events.CollectionChanged -= OnEventsCollectionChanged;
			events.CollectionChanged += OnEventsCollectionChanged;
		}
	}
}
