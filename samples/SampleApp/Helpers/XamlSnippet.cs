using System.Text;
using System.Xml.Linq;

namespace SampleApp.Helpers;

public enum XamlTokenKind
{
    Text,
    Tag,
    Attribute,
    Value,
    Comment,
}

public readonly record struct XamlToken(string Text, XamlTokenKind Kind);

/// <summary>
/// Reads a page's XAML, bundled by SampleApp.csproj as "XamlSources/&lt;Page&gt;.xaml.txt", and
/// re-indents its first Plugin.Maui.Calendar element, so the viewer always shows what the page uses.
/// </summary>
public static class XamlSnippet
{
    const string pluginNamespace = "clr-namespace:Plugin.Maui.Calendar.Controls;assembly=Plugin.Maui.Calendar";
    const string indentUnit = "    ";

    public static async Task<IReadOnlyList<XamlToken>> LoadAsync(string pageName)
    {
        await using var stream = await FileSystem.Current.OpenAppPackageFileAsync($"XamlSources/{pageName}.xaml.txt");
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

        var calendar = document.Descendants().FirstOrDefault(e => e.Name.NamespaceName == pluginNamespace)
            ?? document.Root!;

        List<XamlToken> tokens = [];
        WriteElement(calendar, 0, tokens);
        tokens.RemoveAt(tokens.Count - 1); // trailing line break

        return tokens;
    }

    public static string ToText(IEnumerable<XamlToken> tokens) =>
        string.Concat(tokens.Select(t => t.Text));

    static void WriteElement(XElement element, int depth, List<XamlToken> tokens)
    {
        var indent = Indent(depth);
        var name = QualifiedName(element, element.Name);
        var attributes = element.Attributes().Where(a => !a.IsNamespaceDeclaration).ToList();

        tokens.Add(new(indent, XamlTokenKind.Text));
        tokens.Add(new($"<{name}", XamlTokenKind.Tag));

        // One attribute stays on the tag line, more get a line each, as in the project's XAML
        foreach (var attribute in attributes)
        {
            var separator = attributes.Count == 1 ? " " : Environment.NewLine + Indent(depth + 1);
            tokens.Add(new(separator, XamlTokenKind.Text));
            tokens.Add(new(QualifiedName(element, attribute.Name), XamlTokenKind.Attribute));
            tokens.Add(new("=", XamlTokenKind.Text));
            tokens.Add(new($"\"{Escape(attribute.Value)}\"", XamlTokenKind.Value));
        }

        var children = element.Nodes()
            .Where(n => n is not XText text || !string.IsNullOrWhiteSpace(text.Value))
            .ToList();

        if (children.Count == 0)
        {
            tokens.Add(new(" />", XamlTokenKind.Tag));
            tokens.Add(new(Environment.NewLine, XamlTokenKind.Text));
            return;
        }

        tokens.Add(new(">", XamlTokenKind.Tag));
        tokens.Add(new(Environment.NewLine, XamlTokenKind.Text));

        foreach (var child in children)
        {
            switch (child)
            {
                case XElement childElement:
                    WriteElement(childElement, depth + 1, tokens);
                    break;
                case XComment comment:
                    tokens.Add(new(Indent(depth + 1), XamlTokenKind.Text));
                    tokens.Add(new(FormatComment(comment.Value, Indent(depth + 1)), XamlTokenKind.Comment));
                    tokens.Add(new(Environment.NewLine, XamlTokenKind.Text));
                    break;
                case XText text:
                    tokens.Add(new(Indent(depth + 1) + Escape(text.Value.Trim()) + Environment.NewLine, XamlTokenKind.Text));
                    break;
            }
        }

        tokens.Add(new(indent, XamlTokenKind.Text));
        tokens.Add(new($"</{name}>", XamlTokenKind.Tag));
        tokens.Add(new(Environment.NewLine, XamlTokenKind.Text));
    }

    static string QualifiedName(XElement scope, XName name)
    {
        if (name.Namespace == XNamespace.None)
        {
            return name.LocalName;
        }

        var prefix = scope.GetPrefixOfNamespace(name.Namespace);
        return string.IsNullOrEmpty(prefix) ? name.LocalName : $"{prefix}:{name.LocalName}";
    }

    static string FormatComment(string value, string indent)
    {
        var lines = value.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();

        return lines.Count <= 1
            ? $"<!--  {lines.FirstOrDefault()}  -->"
            : $"<!--{Environment.NewLine}{string.Join(Environment.NewLine, lines.Select(l => indent + indentUnit + l))}{Environment.NewLine}{indent}-->";
    }

    // Icon font glyphs (private use area) and control characters are written back as
    // character references, as they appear in the source, instead of unreadable boxes
    static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var c in value)
        {
            _ = c switch
            {
                '&' => builder.Append("&amp;"),
                '<' => builder.Append("&lt;"),
                '"' => builder.Append("&quot;"),
                _ when char.IsControl(c) || c is >= '\ue000' and <= '\uf8ff' => builder.Append($"&#x{(int)c:x};"),
                _ => builder.Append(c),
            };
        }

        return builder.ToString();
    }

    static string Indent(int depth) => string.Concat(Enumerable.Repeat(indentUnit, depth));
}
