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
	/// Bindable property for TodayOutlineColor
	/// </summary>
	public static readonly BindableProperty TodayOutlineColorProperty = BindableProperty.Create(
		nameof(TodayOutlineColor),
		typeof(Color),
		typeof(Calendar),
		Color.FromArgb("#FF4081"),
		propertyChanged: OnTodayOutlineColorChanged
	);

	/// <summary>
	/// Specifies the color of outline for today's date
	/// </summary>
	public Color TodayOutlineColor
	{
		get => (Color)GetValue(TodayOutlineColorProperty);
		set => SetValue(TodayOutlineColorProperty, value);
	}

	static void OnTodayOutlineColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for
	/// </summary>
	public static readonly BindableProperty TodayTextColorProperty = BindableProperty.Create(
		nameof(TodayTextColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Black,
		propertyChanged: OnTodayTextColorChanged
	);

	/// <summary>
	/// Specifies the color of text for today's date
	/// </summary>
	public Color TodayTextColor
	{
		get => (Color)GetValue(TodayTextColorProperty);
		set => SetValue(TodayTextColorProperty, value);
	}

	static void OnTodayTextColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}


	/// <summary>
	/// Bindable property for TodayFillColor
	/// </summary>
	public static readonly BindableProperty TodayFillColorProperty = BindableProperty.Create(
		nameof(TodayFillColor),
		typeof(Color),
		typeof(Calendar),
		Colors.Transparent,
		propertyChanged: OnTodayFillColorChanged
	);

	/// <summary>
	/// Specifies the fill for today's date
	/// </summary>
	public Color TodayFillColor
	{
		get => (Color)GetValue(TodayFillColorProperty);
		set => SetValue(TodayFillColorProperty, value);
	}

	static void OnTodayFillColorChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is Calendar calendar)
		{
			calendar.UpdateDaysColors();
		}
	}
}
