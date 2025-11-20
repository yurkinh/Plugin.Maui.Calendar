using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for PreviousMonthArrowButtonStyle
	/// </summary>
	public static readonly BindableProperty PreviousMonthArrowButtonStyleProperty = BindableProperty.Create(
		nameof(PreviousMonthArrowButtonStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultPreviousMonthArrowButtonStyle
	);

	/// <summary>
	/// Specifies the style of the previous month arrow button
	/// </summary>
	public Style PreviousMonthArrowButtonStyle
	{
		get => (Style)GetValue(PreviousMonthArrowButtonStyleProperty);
		set => SetValue(PreviousMonthArrowButtonStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for NextMonthArrowButtonStyle
	/// </summary>
	public static readonly BindableProperty NextMonthArrowButtonStyleProperty = BindableProperty.Create(
		nameof(NextMonthArrowButtonStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultNextMonthArrowButtonStyle
	);

	/// <summary>
	/// Specifies the style of the next month arrow button
	/// </summary>
	public Style NextMonthArrowButtonStyle
	{
		get => (Style)GetValue(NextMonthArrowButtonStyleProperty);
		set => SetValue(NextMonthArrowButtonStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for PreviousYearArrowButtonStyle
	/// </summary>
	public static readonly BindableProperty PreviousYearArrowButtonStyleProperty = BindableProperty.Create(
		nameof(PreviousYearArrowButtonStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultPreviousYearArrowButtonStyle
	);

	/// <summary>
	/// Specifies the style of the previous year arrow button
	/// </summary>
	public Style PreviousYearArrowButtonStyle
	{
		get => (Style)GetValue(PreviousYearArrowButtonStyleProperty);
		set => SetValue(PreviousYearArrowButtonStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for NextYearArrowButtonStyle
	/// </summary>
	public static readonly BindableProperty NextYearArrowButtonStyleProperty = BindableProperty.Create(
		nameof(NextYearArrowButtonStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultNextYearArrowButtonStyle
	);

	/// <summary>
	/// Specifies the style of the next year arrow button
	/// </summary>
	public Style NextYearArrowButtonStyle
	{
		get => (Style)GetValue(NextYearArrowButtonStyleProperty);
		set => SetValue(NextYearArrowButtonStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for FooterArrowLabelStyle
	/// </summary>
	public static readonly BindableProperty FooterArrowLabelStyleProperty = BindableProperty.Create(
		nameof(FooterArrowLabelStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultFooterArrowLabelStyle
	);

	/// <summary>
	/// Specifies the style of the footer arrow label
	/// </summary>
	public Style FooterArrowLabelStyle
	{
		get => (Style)GetValue(FooterArrowLabelStyleProperty);
		set => SetValue(FooterArrowLabelStyleProperty, value);
	}
}