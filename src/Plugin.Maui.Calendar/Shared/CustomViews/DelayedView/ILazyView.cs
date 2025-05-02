namespace Plugin.Maui.Calendar.Shared.CustomViews;

public interface ILazyView
{
    View Content { get; set; }

    Color AccentColor { get; }

    bool IsLoaded { get; }

    void LoadView();
}
