namespace Plugin.Maui.Calendar.Shared.CustomViews;

public class DelayedView : LazyView
{
    public static readonly BindableProperty ViewProperty = BindableProperty.Create(
        nameof(View),
        typeof(View),
        typeof(DelayedView),
        default(View));

    public View View
    {
        get => (View)GetValue(ViewProperty);
        set => SetValue(ViewProperty, value);
    }

    public int DelayInMilliseconds { get; set; } = 1000;

    public override void LoadView()
    {
        if (IsLoaded)
        {
            return;
        }

        Task.Run(async () =>
        {
            await Task.Delay(DelayInMilliseconds);
            if (IsLoaded)
            {
                return;
            }

            IsLoaded = true;
            Content = View;
        });
    }
}