namespace Plugin.Maui.Calendar.Controls;

internal class SwipeAwareContainer : ContentView
{   
    readonly SwipeGestureRecognizer leftSwipeGesture;
    readonly SwipeGestureRecognizer rightSwipeGesture;
    readonly SwipeGestureRecognizer upSwipeGesture;
    readonly SwipeGestureRecognizer downSwipeGesture;

    public static readonly BindableProperty SwipeDetectionDisabledProperty =
      BindableProperty.Create(nameof(SwipeDetectionDisabled), typeof(bool), typeof(SwipeAwareContainer), false);

    public bool SwipeDetectionDisabled
    {
        get => (bool)GetValue(SwipeDetectionDisabledProperty);
        set => SetValue(SwipeDetectionDisabledProperty, value);
    }

    internal SwipeAwareContainer() : base()
    {
        leftSwipeGesture = new() { Direction = SwipeDirection.Left };
        rightSwipeGesture = new() { Direction = SwipeDirection.Right };
        upSwipeGesture = new() { Direction = SwipeDirection.Up };
        downSwipeGesture = new() { Direction = SwipeDirection.Down };

        Loaded += SwipeAwareContainer_Loaded;
        Unloaded += SwipeAwareContainer_Unloaded;
    }   

    private void SwipeAwareContainer_Loaded(object sender, EventArgs e)
    {
        GestureRecognizers.Add(leftSwipeGesture);
        GestureRecognizers.Add(rightSwipeGesture);
        GestureRecognizers.Add(upSwipeGesture);
        GestureRecognizers.Add(downSwipeGesture);

        leftSwipeGesture.Swiped += OnSwiped;
        rightSwipeGesture.Swiped += OnSwiped;
        upSwipeGesture.Swiped += OnSwiped;
        downSwipeGesture.Swiped += OnSwiped;       
    }

    private void SwipeAwareContainer_Unloaded(object sender, EventArgs e)
    {
        GestureRecognizers.Remove(leftSwipeGesture);
        GestureRecognizers.Remove(rightSwipeGesture);
        GestureRecognizers.Remove(upSwipeGesture);
        GestureRecognizers.Remove(downSwipeGesture);

        leftSwipeGesture.Swiped -= OnSwiped;
        rightSwipeGesture.Swiped -= OnSwiped;
        upSwipeGesture.Swiped -= OnSwiped;
        downSwipeGesture.Swiped -= OnSwiped;

        Loaded -= SwipeAwareContainer_Loaded;
        Unloaded -= SwipeAwareContainer_Unloaded;
    }

    void OnSwiped(object sender, SwipedEventArgs e)
    {
        switch (e.Direction)
        {
            case SwipeDirection.Left:
                OnSwipeLeft();
                break;
            case SwipeDirection.Right:
                OnSwipeRight();
                break;
            case SwipeDirection.Up:
                OnSwipeUp();
                break;
            case SwipeDirection.Down:
                OnSwipeDown();
                break;
        }
    }

    public event EventHandler SwipedLeft;
    public event EventHandler SwipedRight;
    public event EventHandler SwipedUp;
    public event EventHandler SwipedDown;

    void OnSwipeLeft() => SwipedLeft?.Invoke(this, EventArgs.Empty);
    void OnSwipeRight() => SwipedRight?.Invoke(this, EventArgs.Empty);
    void OnSwipeUp() => SwipedUp?.Invoke(this, EventArgs.Empty);
    void OnSwipeDown() => SwipedDown?.Invoke(this, EventArgs.Empty);
}
