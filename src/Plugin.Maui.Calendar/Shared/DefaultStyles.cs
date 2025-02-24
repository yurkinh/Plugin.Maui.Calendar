namespace Plugin.Maui.Calendar.Styles;

public static class DefaultStyles
{
	#region Base Styles   
	static Style BaseHeaderLabelStyle { get; }
	static Style BaseArrowButtonStyle { get; }

	#endregion


	#region Default Styles
	public static Style DefaultLabelStyle { get; }
	public static Style DefaultMonthLabelStyle { get; }
	public static Style DefaultYearLabelStyle { get; }
	public static Style DefaultPreviousMonthArrowButtonStyle { get; }
	public static Style DefaultNextMonthArrowButtonStyle { get; }
	public static Style DefaultPreviousYearArrowButtonStyle { get; }
	public static Style DefaultNextYearArrowButtonStyle { get; }
	public static Style DefaultFooterArrowLabelStyle { get; }
	public static Style DefaultSelectedDateLabelStyle { get; }
	public static Style DefaultWeekdayTitleStyle { get; }
	public static Style DefaultWeekendTitleStyle { get; }

	#endregion

	static DefaultStyles()
	{
		BaseHeaderLabelStyle = CreateBaseHeaderLabelStyle();
		BaseArrowButtonStyle = CreateBaseArrowButtonStyle();

		DefaultLabelStyle = CreateDefaultLabelStyle();
		DefaultMonthLabelStyle = CreateDefaultMonthLabelStyle();
		DefaultYearLabelStyle = CreateDefaultYearLabelStyle();
		DefaultPreviousMonthArrowButtonStyle = CreateDefaultPreviousMonthArrowButtonStyle();
		DefaultNextMonthArrowButtonStyle = CreateNextDefaultMonthArrowButtonStyle();
		DefaultPreviousYearArrowButtonStyle = CreateDefaultPreviousYearArrowButtonStyle();
		DefaultNextYearArrowButtonStyle = CreateDefaultNextYearArrowButtonStyle();
		DefaultFooterArrowLabelStyle = CreateDefaultFooterArrowLabelStyle();
		DefaultSelectedDateLabelStyle = CreateDefaultSelectedDateLabelStyle();
		DefaultWeekdayTitleStyle = CreateDefaultWeekdayTitleStyle();
		DefaultWeekendTitleStyle = CreateDefaultWeekendTitleStyle();
	}

	static Style CreateBaseHeaderLabelStyle()
	{
		Style style = new(typeof(Label)) { CanCascade = true };
		style.Setters.Add(new Setter() { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
		style.Setters.Add(new Setter() { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.Center });
		style.Setters.Add(new Setter() { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Center });
		style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Color.FromArgb("#2196F3") });
		style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 16 });

		return style;
	}

	static Style CreateBaseArrowButtonStyle()
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

		return style;
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

	static Style CreateDefaultMonthLabelStyle()
	{
		Style style = new(typeof(Label)) { CanCascade = true, BasedOn = BaseHeaderLabelStyle };

		return style;
	}

	static Style CreateDefaultYearLabelStyle()
	{
		Style style = new(typeof(Label)) { CanCascade = true, BasedOn = BaseHeaderLabelStyle };

		return style;
	}

	static Style CreateDefaultPreviousMonthArrowButtonStyle()
	{
		Style style = new(typeof(Button)) { CanCascade = true, BasedOn = BaseArrowButtonStyle };
		style.Setters.Add(new Setter() { Property = Button.TextProperty, Value = "←" });

		return style;
	}

	static Style CreateNextDefaultMonthArrowButtonStyle()
	{
		Style style = new(typeof(Button)) { CanCascade = true, BasedOn = BaseArrowButtonStyle };
		style.Setters.Add(new Setter() { Property = Button.TextProperty, Value = "→" });

		return style;
	}

	static Style CreateDefaultPreviousYearArrowButtonStyle()
	{
		Style style = new(typeof(Button)) { CanCascade = true, BasedOn = BaseArrowButtonStyle };
		style.Setters.Add(new Setter() { Property = Button.TextProperty, Value = "←" });

		return style;
	}

	static Style CreateDefaultNextYearArrowButtonStyle()
	{
		Style style = new(typeof(Button)) { CanCascade = true, BasedOn = BaseArrowButtonStyle };
		style.Setters.Add(new Setter() { Property = Button.TextProperty, Value = "→" });

		return style;
	}

	static Style CreateDefaultFooterArrowLabelStyle()
	{
		Style style = new(typeof(Label)) { CanCascade = true };
		style.Setters.Add(new Setter() { Property = View.MarginProperty, Value = new Thickness(0, 15, 0, 0) });
		style.Setters.Add(new Setter() { Property = View.HorizontalOptionsProperty, Value = LayoutOptions.End });
		style.Setters.Add(new Setter() { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Center });
		style.Setters.Add(new Setter() { Property = Label.VerticalTextAlignmentProperty, Value = LayoutOptions.Center });
		style.Setters.Add(new Setter() { Property = Label.LineBreakModeProperty, Value = LineBreakMode.TailTruncation });
		style.Setters.Add(new Setter() { Property = Label.TextProperty, Value = "↑" });
		style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Colors.Black });
		style.Setters.Add(new Setter() { Property = Label.FontFamilyProperty, Value = "OpenSansSemibold" });
		style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 18 });
		style.Setters.Add(new Setter() { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });

		return style;
	}

	static Style CreateDefaultSelectedDateLabelStyle()
	{
		Style style = new(typeof(Label)) { CanCascade = true };
		style.Setters.Add(new Setter() { Property = View.VerticalOptionsProperty, Value = LayoutOptions.Center });
		style.Setters.Add(new Setter() { Property = Label.VerticalTextAlignmentProperty, Value = LayoutOptions.Center });
		style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Color.FromArgb("#2196F3") });
		style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 16 });
		style.Setters.Add(new Setter() { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });

		return style;
	}

	static Style CreateDefaultWeekdayTitleStyle()
	{
		Style style = new(typeof(Label)) { CanCascade = true };
		style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 18 });
		style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Colors.Black });

		return style;
	}

	static Style CreateDefaultWeekendTitleStyle()
	{
		Style style = new(typeof(Label)) { CanCascade = true };
		style.Setters.Add(new Setter() { Property = Label.FontSizeProperty, Value = 18 });
		style.Setters.Add(new Setter() { Property = Label.TextColorProperty, Value = Colors.Red });
		return style;
	}
}

