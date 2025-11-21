using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	protected override void OnHandlerChanging(HandlerChangingEventArgs args)
	{

		base.OnHandlerChanging(args);

		if (args.NewHandler != null)
		{
			AttachHandler();
		}

		if (args.OldHandler != null)
		{
			DetachHandler();
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
		WeakReferenceMessenger.Default.Register<DayTappedMessage>(this, (r, m) => OnDayTappedHandler(m.Value));

		if (!SwipeDetectionDisabled)
		{
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
	}

	void DetachHandler()
	{
		calendarContainer.SizeChanged -= OnCalendarContainerSizeChanged;
		WeakReferenceMessenger.Default.Unregister<DayTappedMessage>(this);

		if (!SwipeDetectionDisabled && GestureRecognizers.Count > 0)
		{
			leftSwipeGesture.Swiped -= OnSwiped;
			rightSwipeGesture.Swiped -= OnSwiped;
			upSwipeGesture.Swiped -= OnSwiped;
			downSwipeGesture.Swiped -= OnSwiped;

			GestureRecognizers.Remove(leftSwipeGesture);
			GestureRecognizers.Remove(rightSwipeGesture);
			GestureRecognizers.Remove(upSwipeGesture);
			GestureRecognizers.Remove(downSwipeGesture);
		}
		//Todo remove later/when all event and properties will be refactored
		//all this should be done automaticall or not needed
		Dispose();
	}
}
