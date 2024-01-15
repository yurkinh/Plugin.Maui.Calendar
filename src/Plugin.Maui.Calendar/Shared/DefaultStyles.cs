namespace Plugin.Maui.Calendar;

public static class DefaultStyles
{
    public static Style DefaultLabelStyle { get; }
    public static Style DefaultTodayLabelStyle { get; }
    public static Style DefaultDeselectedDayLabelStyle { get; }
    public static Style DefaultTitleDaysLabelStyle { get; }
    public static Style DefaultOtherMonthDayLabelStyle { get; }
    public static Style DefaultDisabledDayLabelStyle { get; }
    public static Style DefaultSelectedTodayLabelStyle { get; }
    public static Style DefaultSelectedDayLabelStyle { get; }
    public static Style DefaultEventIndicatorSelectedLabelStyle { get; }
    public static Style DefaultEventIndicatorLabelStyle { get; }



    static DefaultStyles()
    {
        DefaultLabelStyle = CreateDefaultLabelStyle();
        DefaultOtherMonthDayLabelStyle = CreateDefaultOtherMonthDayLabelStyle();
        DefaultDeselectedDayLabelStyle = CreateDefaultDeselectedDaysLabelStyle();
        DefaultTitleDaysLabelStyle = CreateDefaultTitleLabelStyle();
        DefaultTodayLabelStyle = CreateDefaultTodayLabelStyle();
        DefaultSelectedTodayLabelStyle = CreateDefaultSelectedTodayLabelStyle();
        DefaultDisabledDayLabelStyle = CreateDefaultDisabledDayLabelStyle();
        DefaultSelectedDayLabelStyle = CreateDefaultSelectedDayLabelStyle();
        DefaultEventIndicatorSelectedLabelStyle = CreateDefaultEventIndicatorSelectedLabelStyle();
        DefaultEventIndicatorLabelStyle = CreateDefaultEventIndicatorLabelStyle();
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

    static Style CreateDefaultTodayLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
        return style;
    }

    static Style CreateDefaultDeselectedDaysLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
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

    static Style CreateDefaultDisabledDayLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
        style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Color.FromArgb("#ECECEC") });
        return style;
    }

    static Style CreateDefaultSelectedTodayLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
        return style;
    }

    static Style CreateDefaultSelectedDayLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
        style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Colors.White });
        // Add your custom setters for the selected label style here
        return style;
    }

    static Style CreateDefaultEventIndicatorSelectedLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
        // Add your custom setters for the event indicator selected label style here
        return style;
    }

    static Style CreateDefaultEventIndicatorLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true, BasedOn = DefaultLabelStyle };
        // Add your custom setters for the event indicator label style here
        return style;
    }

}

