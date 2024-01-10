namespace Plugin.Maui.Calendar;

public static class DefaultStyles
{
    public static Style DefaultLabelStyle { get; }
    public static Style DefaultTitleLabelStyle { get; }
    public static Style DefaultOtherMonthDayLabelStyle { get; }

    static DefaultStyles()
    {
        DefaultLabelStyle = CreateDefaultLabelStyle();
        DefaultOtherMonthDayLabelStyle = CreateDefaultOtherMonthDayLabelStyle();
        DefaultTitleLabelStyle = CreateDefaultTitleLabelStyle();
    }

    static Style CreateDefaultLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true };
        style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Colors.Black });
        style.Setters.Add(new Setter() { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center });
        style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 14 });
        style.Setters.Add(new Setter() { Property = Label.LineBreakModeProperty, Value = LineBreakMode.WordWrap });
        style.Setters.Add(new Setter() { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center });
        style.Setters.Add(new Setter() { Property = View.MarginProperty, Value = new Thickness(5, 2, 5, 2) });
        return style;
    }

    static Style CreateDefaultOtherMonthDayLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
        style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Colors.Silver });
        return style;
    }
    static Style CreateDefaultTitleLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true };
        style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Colors.Black });
        style.Setters.Add(new Setter() { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Center });
        style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 18 });
        style.Setters.Add(new Setter() { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center });
        style.Setters.Add(new Setter() { Property = View.MarginProperty, Value = new Thickness(0) });
        return style;
    }

}

