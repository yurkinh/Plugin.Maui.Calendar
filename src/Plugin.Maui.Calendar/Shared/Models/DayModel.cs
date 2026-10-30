using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Models;

sealed class DayModel : ICalendarDay
{
	public event PropertyChangedEventHandler PropertyChanged;

	// TextColor depends on IsToday and IsWeekendColored, both derived from Date, so it must be
	// re-notified here: a reused cell can change date without any other TextColor input changing.
	public DateTime Date
	{
		get;
		set
		{
			if (field == value)
			{
				return;
			}

			field = value;

			// Cached before any notification, so a handler of these notifications reads the new IsToday.
			isToday = value.Date == Today.Date;
			OnPropertyChanged(nameof(Date));
			OnPropertyChanged(nameof(BackgroundColor));
			OnPropertyChanged(nameof(OutlineColor));
			OnPropertyChanged(nameof(TextColor));
			OnPropertyChanged(nameof(IsToday));
			OnPropertyChanged(nameof(IsWeekend));
		}
	}

	public string Day { get; set => SetProperty(ref field, value); }

	public Thickness DayViewBorderMargin { get; set => SetProperty(ref field, value); } = new(0, 0, 0, 0);

	public double DayViewSize { get; set => SetProperty(ref field, value); }

	/// <summary>
	/// Height of the cell: <see cref="Controls.Calendar.DayViewHeight"/>, or <see cref="DayViewSize"/> when that is -1.
	/// </summary>
	public double DayViewHeight { get; set => SetProperty(ref field, value); }

	public float DayViewCornerRadius { get; set => SetProperty(ref field, value); }

	public Style DaysLabelStyle { get; set => SetProperty(ref field, value); } = DefaultStyles.DefaultLabelStyle;

	public ICommand DayTappedCommand { get; set => SetProperty(ref field, value); }

	public bool HasEvents
	{
		get;
		set => SetProperty(ref field, value, [nameof(TextColor), nameof(BackgroundColor), nameof(BackgroundEventIndicator), nameof(BackgroundFullEventColor)]);
	}

	public int EventCount { get; set => SetProperty(ref field, value); }

	public IReadOnlyList<object> Events { get; set => SetProperty(ref field, value); } = [];

	public bool IsRangeStart { get; set => SetProperty(ref field, value); }

	public bool IsRangeEnd { get; set => SetProperty(ref field, value); }

	public bool IsThisMonth
	{
		get;
		set => SetProperty(ref field, value, [nameof(TextColor), nameof(IsVisible), nameof(IsControlVisible), nameof(BackgroundFullEventColor)]);
	}

	public bool IsSelected
	{
		get;
		set => SetProperty(ref field, value, [nameof(TextColor), nameof(BackgroundColor), nameof(OutlineColor), nameof(BackgroundFullEventColor)]);
	}

	public bool AllowDeselect { get; set => SetProperty(ref field, value); }

	public bool OtherMonthIsVisible
	{
		get;
		set => SetProperty(ref field, value, [nameof(IsVisible), nameof(BackgroundFullEventColor)]);
	}

	public bool OtherMonthWeekIsVisible { get; set => SetProperty(ref field, value, [nameof(IsControlVisible)]); }

	public bool IsDisabled { get; set => SetProperty(ref field, value, [nameof(TextColor)]); }

	public Color SelectedTextColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); } = Colors.White;

	public Color SelectedTodayTextColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); } = Colors.Transparent;

	public Color OtherMonthColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); } = Colors.Silver;

	public Color OtherMonthSelectedColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); } = Colors.Gray;

	public Color WeekendDayColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); } = Colors.Transparent;

	public Color DeselectedTextColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); } = Colors.Transparent;

	public Color SelectedBackgroundColor { get; set => SetProperty(ref field, value, [nameof(BackgroundColor)]); } = Color.FromArgb("#2196F3");

	public Color DeselectedBackgroundColor { get; set => SetProperty(ref field, value, [nameof(BackgroundColor)]); } = Colors.Transparent;

	public EventIndicatorType EventIndicatorType
	{
		get;
		set => SetProperty(ref field, value, [nameof(BackgroundEventIndicator), nameof(BackgroundColor), nameof(BackgroundFullEventColor), nameof(EventLayoutDirection)]);
	} = EventIndicatorType.BottomDot;

	public Color EventIndicatorColor
	{
		get;
		set => SetProperty(ref field, value, [nameof(BackgroundColor), nameof(BackgroundFullEventColor)]);
	} = Color.FromArgb("#FF4081");

	public IReadOnlyList<Color> EventColors { get; set => SetProperty(ref field, value); }

	public Color EventIndicatorSelectedColor
	{
		get;
		set => SetProperty(ref field, value, [nameof(BackgroundColor), nameof(BackgroundFullEventColor)]);
	}

	public Color EventIndicatorTextColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); }

	public Color EventIndicatorSelectedTextColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); }

	public Color TodayOutlineColor { get; set => SetProperty(ref field, value, [nameof(OutlineColor)]); } = Color.FromArgb("#FF4081");

	public Color TodayTextColor { get; set => SetProperty(ref field, value, [nameof(TextColor)]); } = Colors.Transparent;

	public Color TodayFillColor { get; set => SetProperty(ref field, value, [nameof(BackgroundColor)]); } = Colors.Transparent;

	public Color DisabledColor { get; set => SetProperty(ref field, value); } = Color.FromArgb("#ECECEC");

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

	// Cached result of Date.Date == Today; updated by the Date setter so that the BackgroundColor,
	// TextColor and OutlineColor getters don't compare dates.
	bool isToday;

	/// <summary>
	/// The date the calendar considers today (from its <see cref="Controls.Calendar.TimeProvider"/>).
	/// Assigned before <see cref="Date"/>, so a new date is compared with the right day. Setting it
	/// raises no change notification: <see cref="RefreshIsToday"/> does that for a date that stays.
	/// </summary>
	internal DateTime Today { get; set; } = DateTime.Today;

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
		Today = today;
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

	// Stores a new value, then raises PropertyChanged for the member and for the values computed from
	// it, in that order. The same value again raises nothing.
	void SetProperty<T>(ref T storage, T value, ReadOnlySpan<string> computedFromIt = default, [CallerMemberName] string propertyName = "")
	{
		if (EqualityComparer<T>.Default.Equals(storage, value))
		{
			return;
		}

		storage = value;
		OnPropertyChanged(propertyName);
		foreach (var computed in computedFromIt)
		{
			OnPropertyChanged(computed);
		}
	}

	void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
