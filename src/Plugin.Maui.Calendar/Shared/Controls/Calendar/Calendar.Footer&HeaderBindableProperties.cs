using Plugin.Maui.Calendar.Styles;


namespace Plugin.Maui.Calendar.Controls;

public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for MonthLabelStyle
	/// </summary>
	public static readonly BindableProperty MonthLabelStyleProperty = BindableProperty.Create(
		nameof(MonthLabelStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultMonthLabelStyle
	);

	/// <summary>
	/// Specifies the style of the month label
	/// </summary>
	public Style MonthLabelStyle
	{
		get => (Style)GetValue(MonthLabelStyleProperty);
		set => SetValue(MonthLabelStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for YearLabelStyle
	/// </summary>
	public static readonly BindableProperty YearLabelStyleProperty = BindableProperty.Create(
		nameof(YearLabelStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultYearLabelStyle
	);

	/// <summary>
	/// Specifies the style of the year label
	/// </summary>
	public Style YearLabelStyle
	{
		get => (Style)GetValue(YearLabelStyleProperty);
		set => SetValue(YearLabelStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for FooterArrowVisible
	/// </summary>
	public static readonly BindableProperty FooterArrowVisibleProperty = BindableProperty.Create(
		nameof(FooterArrowVisible),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Specifies whether the footer expanding arrow is visible
	/// </summary>
	public bool FooterArrowVisible
	{
		get => (bool)GetValue(FooterArrowVisibleProperty);
		set => SetValue(FooterArrowVisibleProperty, value);
	}

	/// <summary>
	/// Bindable property for HeaderSectionVisible
	/// </summary>
	public static readonly BindableProperty HeaderSectionVisibleProperty = BindableProperty.Create(
		nameof(HeaderSectionVisible),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Specifies whether the header section is visible
	/// </summary>
	public bool HeaderSectionVisible
	{
		get => (bool)GetValue(HeaderSectionVisibleProperty);
		set => SetValue(HeaderSectionVisibleProperty, value);
	}


	/// <summary>
	/// Bindable property for FooterSectionVisible
	/// </summary>
	public static readonly BindableProperty FooterSectionVisibleProperty = BindableProperty.Create(
		nameof(FooterSectionVisible),
		typeof(bool),
		typeof(Calendar),
		true
	);

	/// <summary>
	/// Specifies whether the footer section is visible
	/// </summary>
	public bool FooterSectionVisible
	{
		get => (bool)GetValue(FooterSectionVisibleProperty);
		set => SetValue(FooterSectionVisibleProperty, value);
	}

	readonly Lazy<DataTemplate> headerSectionTemplate = new(() => new DataTemplate(() => new DefaultHeaderSection()));
	/// <summary>
	/// Bindable property for HeaderSectionTemplate
	/// </summary>
	public static readonly BindableProperty HeaderSectionTemplateProperty = BindableProperty.Create(
		nameof(HeaderSectionTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		 defaultValueCreator: bindable => ((Calendar)bindable).headerSectionTemplate.Value
	);

	/// <summary>
	/// Data template for the header section
	/// </summary>
	public DataTemplate HeaderSectionTemplate
	{
		get => (DataTemplate)GetValue(HeaderSectionTemplateProperty);
		set => SetValue(HeaderSectionTemplateProperty, value);
	}

	readonly Lazy<DataTemplate> footerSectionTemplate = new(() => new DataTemplate(() => new DefaultFooterSection()));
	/// <summary>
	/// Bindable property for FooterSectionTemplate
	/// </summary>
	public static readonly BindableProperty FooterSectionTemplateProperty = BindableProperty.Create(
		nameof(FooterSectionTemplate),
		typeof(DataTemplate),
		typeof(Calendar),
		defaultValueCreator: bindable => ((Calendar)bindable).footerSectionTemplate.Value
	);

	/// <summary>
	/// Data template for the footer section
	/// </summary>
	public DataTemplate FooterSectionTemplate
	{
		get => (DataTemplate)GetValue(FooterSectionTemplateProperty);
		set => SetValue(FooterSectionTemplateProperty, value);
	}
}
