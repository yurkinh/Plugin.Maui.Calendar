using System.Collections;
using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Controls.Interfaces;
using Plugin.Maui.Calendar.Controls.SelectionEngines;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Styles;
using Plugin.Maui.Calendar.Shared.Extensions;
using System.Collections.Specialized;
using System.Collections.ObjectModel;


namespace Plugin.Maui.Calendar.Controls;
public partial class Calendar : ContentView, IDisposable
{
	/// <summary>
	/// Bindable property for DaysTitleLabelStyle
	/// </summary>
	public static readonly BindableProperty SelectedDateLabelStyleProperty = BindableProperty.Create(
		nameof(SelectedDateLabelStyle),
		typeof(Style),
		typeof(Calendar),
		DefaultStyles.DefaultSelectedDateLabelStyle
	);

	/// <summary>
	/// Specifies the style of day title labels
	/// </summary>
	public Style SelectedDateLabelStyle
	{
		get => (Style)GetValue(SelectedDateLabelStyleProperty);
		set => SetValue(SelectedDateLabelStyleProperty, value);
	}

	/// <summary>
	/// Bindable property for SelectedDateText
	/// </summary>
	public static readonly BindableProperty SelectedDateTextProperty = BindableProperty.Create(
		nameof(SelectedDateText),
		typeof(string),
		typeof(Calendar),
		null
	);

	/// <summary>
	/// Text showing which dates are currently selected
	/// </summary>
	public string SelectedDateText
	{
		get => (string)GetValue(SelectedDateTextProperty);
		set => SetValue(SelectedDateTextProperty, value);
	}


	/// <summary>
	/// Bindable property for SelectedDateTextFormat
	/// </summary>
	public static readonly BindableProperty SelectedDateTextFormatProperty = BindableProperty.Create(
			nameof(SelectedDateTextFormat),
			typeof(string),
			typeof(Calendar),
			"d MMM yyyy"
		);

	/// <summary>
	/// Specifies the format of selected date text
	/// </summary>
	public string SelectedDateTextFormat
	{
		get => (string)GetValue(SelectedDateTextFormatProperty);
		set => SetValue(SelectedDateTextFormatProperty, value);
	}

	/// <summary>
	/// Determines whether tapping on a day from another month should change the displayed month.
	/// </summary>
	public static readonly BindableProperty AutoChangeMonthOnDayTapProperty =
		BindableProperty.Create(
			nameof(AutoChangeMonthOnDayTap),
			typeof(bool),
			typeof(Calendar),
			false
		);

	/// <summary>
	/// Gets or sets whether tapping on a day from another month should change the displayed month.
	/// </summary>
	public bool AutoChangeMonthOnDayTap
	{
		get => (bool)GetValue(AutoChangeMonthOnDayTapProperty);
		set => SetValue(AutoChangeMonthOnDayTapProperty, value);
	}
}
