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
	/// Bindable property for WeekendDayColor
	/// </summary>
	public static readonly BindableProperty WeekendDayColorProperty = BindableProperty.Create(
		nameof(WeekendDayColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black,
		propertyChanged: OnWeekendDayColorChanged
	);

	/// <summary>
	/// Specifies the color of days belonging to a month other than the selected one
	/// </summary>
	public Color WeekendDayColor
	{
		get => (Color)GetValue(WeekendDayColorProperty);
		set => SetValue(WeekendDayColorProperty, value);
	}

	static void OnWeekendDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for OtherMonthDayColor
	/// </summary>
	public static readonly BindableProperty OtherMonthDayColorProperty = BindableProperty.Create(
		nameof(OtherMonthDayColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Silver,
		propertyChanged: OnOtherMonthDayColorChanged
	);

	/// <summary>
	/// Specifies the color of days belonging to a month other than the selected one
	/// </summary>
	public Color OtherMonthDayColor
	{
		get => (Color)GetValue(OtherMonthDayColorProperty);
		set => SetValue(OtherMonthDayColorProperty, value);
	}

	static void OnOtherMonthDayColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}
}
