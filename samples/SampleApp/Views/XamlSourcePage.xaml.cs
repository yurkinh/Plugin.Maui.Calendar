using CommunityToolkit.Maui;
using SampleApp.Helpers;

namespace SampleApp.Views;

/// <summary>
/// Shows the calendar element of a sample page, read from the page's XAML source.
/// </summary>
public partial class XamlSourcePage : ContentPage
{
    readonly string pageName;
    string code = string.Empty;

    public XamlSourcePage(string pageName)
    {
        this.pageName = pageName;
        InitializeComponent();
        SourceLabel.Text = $"{pageName}.xaml";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (CodeLabel.FormattedText is not null)
        {
            return;
        }

        try
        {
            var tokens = await XamlSnippet.LoadAsync(pageName);
            code = XamlSnippet.ToText(tokens);
            CodeLabel.FormattedText = CreateFormattedText(tokens);
            CopyButton.IsEnabled = true;
        }
        catch (Exception ex) when (ex is FileNotFoundException or System.Xml.XmlException)
        {
            CodeLabel.Text = $"The XAML of {pageName} is not available: {ex.Message}";
        }
    }

    static FormattedString CreateFormattedText(IEnumerable<XamlToken> tokens)
    {
        var formattedText = new FormattedString();

        // Neighbouring tokens of one kind share a span: fewer spans lay out faster
        foreach (var group in Merge(tokens))
        {
            var span = new Span { Text = group.Text };
            var color = GetThemeColor(group.Kind switch
            {
                XamlTokenKind.Tag => "CodeTagColor",
                XamlTokenKind.Attribute => "CodeAttributeColor",
                XamlTokenKind.Value => "CodeValueColor",
                XamlTokenKind.Comment => "CodeCommentColor",
                _ => "TextPrimaryColor",
            });
            span.SetAppThemeColor(Span.TextColorProperty, color.Light, color.Dark);
            formattedText.Spans.Add(span);
        }

        return formattedText;
    }

    static IEnumerable<XamlToken> Merge(IEnumerable<XamlToken> tokens)
    {
        XamlToken? pending = null;

        foreach (var token in tokens)
        {
            if (pending is { } previous && previous.Kind == token.Kind)
            {
                pending = previous with { Text = previous.Text + token.Text };
                continue;
            }

            if (pending is { } completed)
            {
                yield return completed;
            }

            pending = token;
        }

        if (pending is { } last)
        {
            yield return last;
        }
    }

    static AppThemeColor GetThemeColor(string key) =>
        (AppThemeColor)Application.Current!.Resources[key];

    async void OnCopyClicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(code);

        CopyButton.Text = "Copied";
        await Task.Delay(1500);
        CopyButton.Text = "Copy";
    }

    async void OnCloseClicked(object sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
