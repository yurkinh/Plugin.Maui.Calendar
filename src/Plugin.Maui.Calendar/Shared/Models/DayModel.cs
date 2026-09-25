using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Models;

sealed partial class DayModel : ObservableObject, ICalendarDay
{
	// TextColor depends on IsToday and IsWeekendColored, both derived from Date, so it must be
	// re-notified here: a reused cell can change date without any other TextColor input changing.
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	[NotifyPropertyChangedFor(nameof(OutlineColor))]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	[NotifyPropertyChangedFor(nameof(IsToday))]
	[NotifyPropertyChangedFor(nameof(IsWeekend))]
	public partial DateTime Date { get; set; }

	[ObservableProperty]
	public partial string Day { get; set; }

	[ObservableProperty]
	public partial Thickness DayViewBorderMargin { get; set; } = new(0, 0, 0, 0);

	[ObservableProperty]
	double dayViewSize;

	[ObservableProperty]
	public partial float DayViewCornerRadius { get; set; }

	[ObservableProperty]
	public partial Style DaysLabelStyle { get; set; } = DefaultStyles.DefaultLabelStyle;

	[ObservableProperty]
	public partial ICommand DayTappedCommand { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(TextColor),
		nameof(BackgroundColor),
		nameof(BackgroundEventIndicator),
		nameof(BackgroundFullEventColor)
	)]
	public partial bool HasEvents { get; set; }

	[ObservableProperty]
	public partial int EventCount { get; set; }

	[ObservableProperty]
	public partial IReadOnlyList<object> Events { get; set; } = [];

	[ObservableProperty]
	public partial bool IsRangeStart { get; set; }

	[ObservableProperty]
	public partial bool IsRangeEnd { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor), nameof(IsVisible), nameof(IsControlVisible), nameof(BackgroundFullEventColor))]
	public partial bool IsThisMonth { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(TextColor),
		nameof(BackgroundColor),
		nameof(OutlineColor),
		nameof(BackgroundFullEventColor)
	)]
	public partial bool IsSelected { get; set; }

	[ObservableProperty]
	public partial bool AllowDeselect { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsVisible), nameof(BackgroundFullEventColor))]
	public partial bool OtherMonthIsVisible { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsControlVisible))]
	public partial bool OtherMonthWeekIsVisible { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial bool IsDisabled { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial Color SelectedTextColor { get; set; } = Colors.White;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial Color SelectedTodayTextColor { get; set; } = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial Color OtherMonthColor { get; set; } = Colors.Silver;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial Color OtherMonthSelectedColor { get; set; } = Colors.Gray;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial Color WeekendDayColor { get; set; } = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial Color DeselectedTextColor { get; set; } = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	public partial Color SelectedBackgroundColor { get; set; } = Color.FromArgb("#2196F3");

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	public partial Color DeselectedBackgroundColor { get; set; } = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundEventIndicator),
		nameof(BackgroundColor),
		nameof(BackgroundFullEventColor),
		nameof(EventLayoutDirection)
	)]
	public partial EventIndicatorType EventIndicatorType { get; set; } = EventIndicatorType.BottomDot;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundColor),
		nameof(BackgroundFullEventColor)
	)]
	public partial Color EventIndicatorColor { get; set; } = Color.FromArgb("#FF4081");

	[ObservableProperty]
	public partial IReadOnlyList<Color> EventColors { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundColor),
		nameof(BackgroundFullEventColor)
	)]
	public partial Color EventIndicatorSelectedColor { get; set; }

	[ObservableProperty]
	public partial Color EventIndicatorTextColor { get; set; }

	[ObservableProperty]
	public partial Color EventIndicatorSelectedTextColor { get; set; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(OutlineColor))]
	public partial Color TodayOutlineColor { get; set; } = Color.FromArgb("#FF4081");

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	public partial Color TodayTextColor { get; set; } = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	public partial Color TodayFillColor { get; set; } = Colors.Transparent;

	[ObservableProperty]
	public partial Color DisabledColor { get; set; } = Color.FromArgb("#ECECEC");

	// Applies to every cell, not only to days with events: the (possibly empty) dot row then sits
	// above the day number in all cells, so the numbers stay aligned across the grid.
	public FlexDirection EventLayoutDirection => EventIndicatorType == EventIndicatorType.TopDot ? FlexDirection.ColumnReverse : FlexDirection.Column;

	public bool BackgroundEventIndicator => HasEvents && EventIndicatorType == EventIndicatorType.Background;

	// Painted on the whole cell (the DayView), which stays visible for a hidden other-month day,
	// so a hidden day must not paint its event color either.
	public Color BackgroundFullEventColor => IsVisible && HasEvents && EventIndicatorType == EventIndicatorType.BackgroundFull ? EventIndicatorColor : Colors.Transparent;

	public Color OutlineColor => IsToday && !IsSelected ? TodayOutlineColor : Colors.Transparent;

	public Color BackgroundColor
	{
		get
		{
			if (!IsVisible || IsDisabled)
			{
				return DeselectedBackgroundColor;
			}

			return (BackgroundEventIndicator, IsSelected, IsToday) switch
			{
				(true, false, _) => EventIndicatorColor,
				(true, true, _) => EventIndicatorSelectedColor,
				(false, true, _) => SelectedBackgroundColor,
				(false, false, true) => TodayFillColor,
				(_, _, _) => DeselectedBackgroundColor
			};
		}
	}

	public Color TextColor
	{
		get
		{
			if (!IsVisible)
			{
				return OtherMonthColor;
			}

			return (IsDisabled, IsSelected, HasEvents, IsThisMonth, IsToday, IsWeekendColored) switch
			{
				(true, _, _, _, _, _) => DisabledColor,
				(false, true, false, true, true, _)
					=> SelectedTodayTextColor == Colors.Transparent
						? SelectedTextColor
						: SelectedTodayTextColor,
				(false, true, false, true, false, _) => SelectedTextColor,
				(false, true, true, true, _, _) => EventIndicatorSelectedTextColor,
				(false, false, true, true, _, _) => EventIndicatorTextColor,
				(false, false, _, false, _, _) => OtherMonthColor,
				(false, true, _, false, _, _) => OtherMonthSelectedColor,
				(false, false, false, true, true, _)
					=> TodayTextColor == Colors.Transparent ? DeselectedTextColor : TodayTextColor,
				(false, _, _, _, _, true) => WeekendDayColor,
				(false, false, false, true, false, _) => DeselectedTextColor,
			};
		}
	}

	public bool IsVisible => IsThisMonth || OtherMonthIsVisible;

	public bool IsControlVisible => IsThisMonth || OtherMonthWeekIsVisible;
	
	// Cached result of Date.Date == DateTime.Today; updated in OnDateChanged via the
	// MVVM Toolkit partial hook so that BackgroundColor, TextColor and OutlineColor
	// getters never call DateTime.Today more than once per Date assignment.
	bool isToday;

	// Runs before the generated setter raises PropertyChanged for Date and its dependents
	// (including IsToday), so the cache is set silently here.
	partial void OnDateChanged(DateTime value)
	{
		isToday = value.Date == DateTime.Today;
	}

	public bool IsToday => isToday;

	/// <summary>
	/// Recomputes <see cref="IsToday"/> against <paramref name="today"/> and raises
	/// PropertyChanged for it and every color that depends on it when the value changes.
	/// Needed because the cache is otherwise only refreshed when <see cref="Date"/> changes,
	/// so a cell that keeps its date across midnight would keep the previous day's state.
	/// </summary>
	/// <returns>Whether <see cref="IsToday"/> changed.</returns>
	internal bool RefreshIsToday(DateTime today)
	{
		var value = Date.Date == today.Date;
		if (isToday == value)
		{
			return false;
		}

		isToday = value;
		OnPropertyChanged(nameof(IsToday));
		OnPropertyChanged(nameof(BackgroundColor));
		OnPropertyChanged(nameof(OutlineColor));
		OnPropertyChanged(nameof(TextColor));
		return true;
	}

	public bool IsWeekend => Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

	// Drives the WeekendDayColor branch of TextColor: a weekend day is only coloured
	// differently when a visible WeekendDayColor has been set.
	public bool IsWeekendColored => IsWeekend && WeekendDayColor != Colors.Transparent;
}
