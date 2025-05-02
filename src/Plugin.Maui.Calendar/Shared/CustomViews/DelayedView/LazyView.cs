namespace Plugin.Maui.Calendar.Shared.CustomViews;

public abstract class LazyView : ContentView, ILazyView, IDisposable
{
    public static readonly BindableProperty AccentColorProperty = BindableProperty.Create(
        nameof(AccentColor),
        typeof(Color),
        typeof(ILazyView),
        Colors.Red,
        propertyChanged: AccentColorChanged);

    public static readonly BindableProperty UseActivityIndicatorProperty = BindableProperty.Create(
        nameof(UseActivityIndicator),
        typeof(bool),
        typeof(ILazyView),
        false,
        propertyChanged: UseActivityIndicatorChanged);

    public static readonly BindableProperty AnimateProperty = BindableProperty.Create(
        nameof(Animate),
        typeof(bool),
        typeof(ILazyView),
        false);

    public Color AccentColor
    {
        get => (Color)GetValue(AccentColorProperty);
        set => SetValue(AccentColorProperty, value);
    }

    public bool UseActivityIndicator
    {
        get => (bool)GetValue(UseActivityIndicatorProperty);
        set => SetValue(UseActivityIndicatorProperty, value);
    }

    public bool Animate
    {
        get => (bool)GetValue(AnimateProperty);
        set => SetValue(AnimateProperty, value);
    }

    public new bool IsLoaded { get; protected set; }

    public abstract void LoadView();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Content is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    protected override void OnBindingContextChanged()
    {
        if (Content != null && !(Content is ActivityIndicator))
        {
            Content.BindingContext = BindingContext;
        }
    }

    static void AccentColorChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var lazyView = (ILazyView)bindable;
        if (lazyView.Content is ActivityIndicator activityIndicator)
        {
            activityIndicator.Color = (Color)newvalue;
        }
    }

    static void UseActivityIndicatorChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var lazyView = (ILazyView)bindable;
        bool useActivityIndicator = (bool)newvalue;

        if (useActivityIndicator)
        {
            lazyView.Content = new ActivityIndicator
            {
                Color = lazyView.AccentColor,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsRunning = true,
            };
        }
    }
}
