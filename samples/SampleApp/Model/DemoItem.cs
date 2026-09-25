namespace SampleApp.Model;

/// <summary>
/// One row of the sample list on the main page.
/// </summary>
public class DemoItem(string title, string description, string glyph, Func<Task> open)
{
    public string Title { get; } = title;

    public string Description { get; } = description;

    /// <summary>
    /// Font Awesome (solid) glyph of the row icon.
    /// </summary>
    public string Glyph { get; } = glyph;

    /// <summary>
    /// False for the last row of a group, which needs no divider below it.
    /// </summary>
    public bool ShowDivider { get; internal set; } = true;

    // AsyncRelayCommand ignores taps while the page is still opening
    public IAsyncRelayCommand OpenCommand { get; } = new AsyncRelayCommand(open);
}
