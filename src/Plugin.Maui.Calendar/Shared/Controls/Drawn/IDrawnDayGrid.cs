using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls.Drawn;

/// <summary>
/// What the calendar asks of a drawn day grid, whatever draws it: DrawnUI (<see cref="DrawnDayGrid"/>)
/// or Microsoft.Maui.Graphics (<see cref="GraphicsDayGrid"/>, the proof of concept of the migration
/// away from DrawnUI). Both exist side by side until the migration is done.
/// </summary>
interface IDrawnDayGrid
{
	/// <summary>Raised with the day when a day is tapped.</summary>
	Action<DayModel> DayTapped { get; set; }

	/// <summary>Raised when a swipe over the grid ends, with its direction.</summary>
	Action<SwipeDirection> Swiped { get; set; }

	/// <summary>Whether the week runs from right to left.</summary>
	bool IsRightToLeft { get; set; }

	/// <summary>The calendar's formatting culture.</summary>
	System.Globalization.CultureInfo Culture { get; set; }

	/// <summary>The style of the day numbers (<c>DaysLabelStyle</c>).</summary>
	Style DayLabelStyle { get; set; }

	/// <summary>Shows the days of a new layout, in grid order.</summary>
	void SetDays(IReadOnlyList<DayModel> days);

	/// <summary>Lets go of the days while the calendar shows the native grid.</summary>
	void ReleaseDays();

	/// <summary>Shows the day-of-week titles, the weekend ones with <paramref name="weekendStyle"/>.</summary>
	void SetTitles(IReadOnlyList<string> texts, IReadOnlyList<bool> weekend, Style style, Style weekendStyle);

	/// <summary>Shows a band behind the weekend days of every week; a transparent color shows none.</summary>
	void SetWeekendBands(Color color, double cornerRadius, DayOfWeek firstDayOfWeek);
}
