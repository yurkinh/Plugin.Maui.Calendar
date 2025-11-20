using System.Windows.Input;
using Plugin.Maui.Calendar.Shared.Extensions;


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

	public string LocalizedYear => UseNativeDigits ? ShownDate.Year.ToNativeDigitString(Culture) : ShownDate.Year.ToString(Culture);
}
