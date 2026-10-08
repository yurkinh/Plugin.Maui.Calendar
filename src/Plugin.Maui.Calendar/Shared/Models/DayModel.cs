using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Models;

sealed class DayModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	DateTime date;
	public DateTime Date
	{
		get => date;
		set
		{
			if (date == value)
			{
				return;
			}

			date = value;
			// refresh the cache before notifying so dependent getters see the new value
			isToday = value.Date == DateTime.Today;
			OnPropertyChanged();
			OnPropertyChanged(nameof(BackgroundColor));
			OnPropertyChanged(nameof(OutlineColor));
		}
	}

	string day;
	public string Day
	{
		get => day;
		set => SetProperty(ref day, value);
	}

	Thickness dayViewBorderMargin = new(0, 0, 0, 0);
	public Thickness DayViewBorderMargin
	{
		get => dayViewBorderMargin;
		set => SetProperty(ref dayViewBorderMargin, value);
	}

	double dayViewSize;
	public double DayViewSize
	{
		get => dayViewSize;
		set => SetProperty(ref dayViewSize, value);
	}

	float dayViewCornerRadius;
	public float DayViewCornerRadius
	{
		get => dayViewCornerRadius;
		set => SetProperty(ref dayViewCornerRadius, value);
	}

	Style daysLabelStyle = DefaultStyles.DefaultLabelStyle;
	public Style DaysLabelStyle
	{
		get => daysLabelStyle;
		set => SetProperty(ref daysLabelStyle, value);
	}

	ICommand dayTappedCommand;
	public ICommand DayTappedCommand
	{
		get => dayTappedCommand;
		set => SetProperty(ref dayTappedCommand, value);
	}

	bool hasEvents;
	public bool HasEvents
	{
		get => hasEvents;
		set
		{
			if (SetProperty(ref hasEvents, value))
			{
				OnPropertyChanged(nameof(TextColor));
				OnPropertyChanged(nameof(BackgroundColor));
				OnPropertyChanged(nameof(BackgroundEventIndicator));
				OnPropertyChanged(nameof(BackgroundFullEventColor));
			}
		}
	}

	bool isThisMonth;
	public bool IsThisMonth
	{
		get => isThisMonth;
		set
		{
			if (SetProperty(ref isThisMonth, value))
			{
				OnPropertyChanged(nameof(TextColor));
				OnPropertyChanged(nameof(IsVisible));
				OnPropertyChanged(nameof(IsControlVisible));
			}
		}
	}

	bool isSelected;
	public bool IsSelected
	{
		get => isSelected;
		set
		{
			if (SetProperty(ref isSelected, value))
			{
				OnPropertyChanged(nameof(TextColor));
				OnPropertyChanged(nameof(BackgroundColor));
				OnPropertyChanged(nameof(OutlineColor));
				OnPropertyChanged(nameof(BackgroundFullEventColor));
			}
		}
	}

	bool allowDeselect;
	public bool AllowDeselect
	{
		get => allowDeselect;
		set => SetProperty(ref allowDeselect, value);
	}

	bool otherMonthIsVisible;
	public bool OtherMonthIsVisible
	{
		get => otherMonthIsVisible;
		set
		{
			if (SetProperty(ref otherMonthIsVisible, value))
			{
				OnPropertyChanged(nameof(IsVisible));
			}
		}
	}

	bool otherMonthWeekIsVisible;
	public bool OtherMonthWeekIsVisible
	{
		get => otherMonthWeekIsVisible;
		set
		{
			if (SetProperty(ref otherMonthWeekIsVisible, value))
			{
				OnPropertyChanged(nameof(IsControlVisible));
			}
		}
	}

	bool isDisabled;
	public bool IsDisabled
	{
		get => isDisabled;
		set => SetPropertyAndNotify(ref isDisabled, value, nameof(TextColor));
	}

	Color selectedTextColor = Colors.White;
	public Color SelectedTextColor
	{
		get => selectedTextColor;
		set => SetPropertyAndNotify(ref selectedTextColor, value, nameof(TextColor));
	}

	Color selectedTodayTextColor = Colors.Transparent;
	public Color SelectedTodayTextColor
	{
		get => selectedTodayTextColor;
		set => SetPropertyAndNotify(ref selectedTodayTextColor, value, nameof(TextColor));
	}

	Color otherMonthColor = Colors.Silver;
	public Color OtherMonthColor
	{
		get => otherMonthColor;
		set => SetPropertyAndNotify(ref otherMonthColor, value, nameof(TextColor));
	}

	Color otherMonthSelectedColor = Colors.Gray;
	public Color OtherMonthSelectedColor
	{
		get => otherMonthSelectedColor;
		set => SetPropertyAndNotify(ref otherMonthSelectedColor, value, nameof(TextColor));
	}

	Color weekendDayColor = Colors.Transparent;
	public Color WeekendDayColor
	{
		get => weekendDayColor;
		set => SetPropertyAndNotify(ref weekendDayColor, value, nameof(TextColor));
	}

	Color deselectedTextColor = Colors.Transparent;
	public Color DeselectedTextColor
	{
		get => deselectedTextColor;
		set => SetPropertyAndNotify(ref deselectedTextColor, value, nameof(TextColor));
	}

	Color selectedBackgroundColor = Color.FromArgb("#2196F3");
	public Color SelectedBackgroundColor
	{
		get => selectedBackgroundColor;
		set => SetPropertyAndNotify(ref selectedBackgroundColor, value, nameof(BackgroundColor));
	}

	Color deselectedBackgroundColor = Colors.Transparent;
	public Color DeselectedBackgroundColor
	{
		get => deselectedBackgroundColor;
		set => SetPropertyAndNotify(ref deselectedBackgroundColor, value, nameof(BackgroundColor));
	}

	EventIndicatorType eventIndicatorType = EventIndicatorType.BottomDot;
	public EventIndicatorType EventIndicatorType
	{
		get => eventIndicatorType;
		set
		{
			if (SetProperty(ref eventIndicatorType, value))
			{
				OnPropertyChanged(nameof(BackgroundEventIndicator));
				OnPropertyChanged(nameof(BackgroundColor));
			}
		}
	}

	Color eventIndicatorColor = Color.FromArgb("#FF4081");
	public Color EventIndicatorColor
	{
		get => eventIndicatorColor;
		set
		{
			if (SetProperty(ref eventIndicatorColor, value))
			{
				OnPropertyChanged(nameof(BackgroundColor));
				OnPropertyChanged(nameof(BackgroundFullEventColor));
			}
		}
	}

	List<Color> eventColors;
	public List<Color> EventColors
	{
		get => eventColors;
		set => SetProperty(ref eventColors, value);
	}

	Color eventIndicatorSelectedColor;
	public Color EventIndicatorSelectedColor
	{
		get => eventIndicatorSelectedColor;
		set
		{
			if (SetProperty(ref eventIndicatorSelectedColor, value))
			{
				OnPropertyChanged(nameof(BackgroundColor));
				OnPropertyChanged(nameof(BackgroundFullEventColor));
			}
		}
	}

	Color eventIndicatorTextColor;
	public Color EventIndicatorTextColor
	{
		get => eventIndicatorTextColor;
		set => SetProperty(ref eventIndicatorTextColor, value);
	}

	Color eventIndicatorSelectedTextColor;
	public Color EventIndicatorSelectedTextColor
	{
		get => eventIndicatorSelectedTextColor;
		set => SetProperty(ref eventIndicatorSelectedTextColor, value);
	}

	Color todayOutlineColor = Color.FromArgb("#FF4081");
	public Color TodayOutlineColor
	{
		get => todayOutlineColor;
		set => SetPropertyAndNotify(ref todayOutlineColor, value, nameof(OutlineColor));
	}

	Color todayTextColor = Colors.Transparent;
	public Color TodayTextColor
	{
		get => todayTextColor;
		set => SetPropertyAndNotify(ref todayTextColor, value, nameof(TextColor));
	}

	Color todayFillColor = Colors.Transparent;
	public Color TodayFillColor
	{
		get => todayFillColor;
		set => SetPropertyAndNotify(ref todayFillColor, value, nameof(BackgroundColor));
	}

	Color disabledColor = Color.FromArgb("#ECECEC");
	public Color DisabledColor
	{
		get => disabledColor;
		set => SetProperty(ref disabledColor, value);
	}

	public FlexDirection EventLayoutDirection => (HasEvents && EventIndicatorType == EventIndicatorType.TopDot) ? FlexDirection.ColumnReverse : FlexDirection.Column;

	public bool BackgroundEventIndicator => HasEvents && EventIndicatorType == EventIndicatorType.Background;

	public Color BackgroundFullEventColor => HasEvents && EventIndicatorType == EventIndicatorType.BackgroundFull ? EventIndicatorColor : Colors.Transparent;

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

			return (IsDisabled, IsSelected, HasEvents, IsThisMonth, IsToday, IsWeekend) switch
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

	// Cached result of Date.Date == DateTime.Today; updated in the Date setter so that
	// BackgroundColor, TextColor and OutlineColor getters never call DateTime.Today
	// more than once per Date assignment.
	bool isToday;

	bool IsToday => isToday;

	public bool IsWeekend => (Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday) && WeekendDayColor != Colors.Transparent;

	bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}

		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	void SetPropertyAndNotify<T>(ref T field, T value, string dependentPropertyName, [CallerMemberName] string propertyName = null)
	{
		if (SetProperty(ref field, value, propertyName))
		{
			OnPropertyChanged(dependentPropertyName);
		}
	}

	void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
