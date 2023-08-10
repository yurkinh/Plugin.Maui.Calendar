namespace Plugin.Maui.Calendar;

public static class DefaultStyles
{
    public static Style DefaultLabelStyle { get; }

    static DefaultStyles()
    {
        DefaultLabelStyle = CreateDefaultLabelStyle();        
    }

    static Style CreateDefaultLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true };
        style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 14d });
        style.Setters.Add(new Setter() { Property = VisualElement.BackgroundColorProperty, Value = Colors.Transparent });

        return style;
    }

}

