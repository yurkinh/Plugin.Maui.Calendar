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
	/// When executed calendar moves to previous week/month.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand PrevLayoutUnitCommand { get; }

	/// <summary>
	/// When executed calendar moves to next week/month.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand NextLayoutUnitCommand { get; }

	/// <summary>
	/// When executed calendar moves to previous year.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand PrevYearCommand { get; }

	/// <summary>
	/// When executed calendar moves to next year.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand NextYearCommand { get; }

	/// <summary>
	/// When executed shows/hides the calendar's current month days view.
	/// Read only command to use in your <see cref="HeaderSectionTemplate"/> or <see cref="FooterSectionTemplate"/>
	/// </summary>
	public ICommand ShowHideCalendarCommand { get; }
}
