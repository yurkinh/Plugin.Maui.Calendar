namespace SampleApp.Model;

/// <summary>
/// A titled section of the sample list on the main page.
/// </summary>
public class DemoGroup
{
    public DemoGroup(string title, IReadOnlyList<DemoItem> items)
    {
        Title = title;
        Items = items;
        items[^1].ShowDivider = false;
    }

    public string Title { get; }

    public IReadOnlyList<DemoItem> Items { get; }
}
