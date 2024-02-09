



namespace Plugin.Maui.Calendar.Styles;

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
    public static Style DefaultArrowButtonPrevStyle { get; }
    public static Style DefaultArrowButtonNextStyle { get; }
    public static Style DefaultFooterArrowButtonStyle { get; }
    public static Style DefaultFooterLabelStyle { get; }
    public static Style DefaultHeaderLabelStyle { get; }


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
        DefaultArrowButtonPrevStyle = CreateDefaultArrowButtonPrevStyle();
        DefaultArrowButtonNextStyle = CreateDefaultArrowButtonNextStyle();
        DefaultFooterArrowButtonStyle = CreateDefaultFooterArrowButtonStyle();
        DefaultFooterLabelStyle = CreateDefaultFooterLabelStyle();
        DefaultHeaderLabelStyle = CreateDefaultHeaderLabelStyle();
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

    private static Style CreateDefaultArrowButtonPrevStyle()
    {
        Style style = new(typeof(Button)) { CanCascade = true };
        style.Setters.Add(new Setter() { Property = Button.PaddingProperty, Value = new Thickness(0) });
        style.Setters.Add(new Setter() { Property = VisualElement.WidthRequestProperty, Value = 36 });
        style.Setters.Add(new Setter() { Property = VisualElement.HeightRequestProperty, Value = 36 });
        style.Setters.Add(new Setter() { Property = Button.CornerRadiusProperty, Value = 18 });
        style.Setters.Add(new Setter() { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.Center });
        style.Setters.Add(new Setter() { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Center });
        style.Setters.Add(new Setter() { Property = VisualElement.BackgroundColorProperty, Value = Colors.White });
        style.Setters.Add(new Setter() { Property = Button.BorderColorProperty, Value = Colors.Black });
        style.Setters.Add(new Setter() { Property = Button.BorderWidthProperty, Value = 1 });
        style.Setters.Add(new Setter() { Property = Button.FontAttributesProperty, Value = FontAttributes.Bold });
        style.Setters.Add(new Setter() { Property = Button.FontSizeProperty, Value = 14 });
        style.Setters.Add(new Setter() { Property = Button.FontFamilyProperty, Value = "OpenSansSemibold" });
        style.Setters.Add(new Setter() { Property = Button.TextColorProperty, Value = Colors.Black });
        style.Setters.Add(new Setter() { Property = Button.TextProperty, Value = "←" });

        return style;
    }

    private static Style CreateDefaultArrowButtonNextStyle()
    {
        Style style = new(typeof(Button)) { CanCascade = true, BasedOn = DefaultArrowButtonPrevStyle };
        style.Setters.Add(new Setter() { Property = Button.TextProperty, Value = "→" });

        return style;
    }

    private static Style CreateDefaultFooterArrowButtonStyle()
    {
        Style style = new(typeof(Button)) { CanCascade = true, BasedOn = DefaultArrowButtonPrevStyle };
        style.Setters.Add(new Setter() { Property = View.MarginProperty, Value = new Thickness(0, 15, 0, 0) });
        style.Setters.Add(new Setter() { Property = Button.BorderColorProperty, Value = Colors.Transparent });
        style.Setters.Add(new Setter() { Property = Button.BorderWidthProperty, Value = 0 });
        style.Setters.Add(new Setter() { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.End });
        style.Setters.Add(new Setter() { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Center });
        style.Setters.Add(new Setter() { Property = Button.LineBreakModeProperty, Value = LineBreakMode.TailTruncation });
        style.Setters.Add(new Setter() { Property = Button.TextProperty, Value = "↑" });

        return style;
    }

    private static Style CreateDefaultFooterLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true };
        style.Setters.Add(new Setter() { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
        style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 16 });
        style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Color.FromArgb("#2196F3") });
        style.Setters.Add(new Setter() { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Center });
        style.Setters.Add(new Setter() { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Center });
        style.Setters.Add(new Setter() { Property = Label.FontFamilyProperty, Value = "OpenSansSemibold" });

        return style;
    }

    private static Style CreateDefaultHeaderLabelStyle()
    {
        Style style = new(typeof(Label)) { CanCascade = true };
        style.Setters.Add(new Setter() { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
        style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 18 });
        style.Setters.Add(new Setter() { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Center });
        style.Setters.Add(new Setter() { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.Center });
        style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = "#2196F3" });

        return style;
    }
}