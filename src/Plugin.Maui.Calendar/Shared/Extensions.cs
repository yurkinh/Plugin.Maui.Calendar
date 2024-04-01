namespace Plugin.Maui.Calendar;

internal static class Extensions
{
    internal static string Capitalize(this string source)
    {
        if (source.Length == 0)
            return source;

        return char.ToUpperInvariant(source[0]) + source[1..];
    }

    internal static object CreateContent(this DataTemplate dataTemplate, object itemModel)
    {
        if (dataTemplate is DataTemplateSelector templateSelector)
        {
            var template = templateSelector.SelectTemplate(itemModel, null);
            template.SetValue(BindableObject.BindingContextProperty, itemModel);

            return template.CreateContent();
        }

        dataTemplate.SetValue(BindableObject.BindingContextProperty, itemModel);
        return dataTemplate.CreateContent();
    }

    public static T GetSetterValue<T>(this Style style, BindableProperty property)
    {
        var setter = style.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == property);
        return setter != null ? (T)setter.Value : default;
    }
}
