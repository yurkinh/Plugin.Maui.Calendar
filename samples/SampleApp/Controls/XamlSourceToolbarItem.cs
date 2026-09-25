using CommunityToolkit.Maui;
using SampleApp.Views;

namespace SampleApp.Controls;

/// <summary>
/// Toolbar button of a sample page: opens the XAML of the page's calendar.
/// </summary>
public class XamlSourceToolbarItem : ToolbarItem
{
    public XamlSourceToolbarItem()
    {
        Text = "XAML";
        IconImageSource = CreateIcon();
        Command = new AsyncRelayCommand(OpenAsync);
    }

    static FontImageSource CreateIcon()
    {
        var icon = new FontImageSource
        {
            FontFamily = "FontAwesomeSolid",
            Glyph = "\uf121",
            Size = 18,
        };

        var accent = (AppThemeColor)Application.Current!.Resources["AccentColor"];
        icon.SetAppThemeColor(FontImageSource.ColorProperty, accent.Light, accent.Dark);

        return icon;
    }

    static Task OpenAsync()
    {
        var page = Shell.Current.CurrentPage;
        return page.Navigation.PushModalAsync(new XamlSourcePage(page.GetType().Name));
    }
}
