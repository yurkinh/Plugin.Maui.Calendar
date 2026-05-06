using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Models;

sealed partial class DayModel : ObservableObject
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	[NotifyPropertyChangedFor(nameof(OutlineColor))]
	DateTime date;

	[ObservableProperty]
	string day;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(DayIndicatorViewMargin))] 
	Thickness dayViewBorderMargin = new(8);

	[ObservableProperty]
	double dayViewSize;

	[ObservableProperty]
	double dayIndicatorViewSize;

	[ObservableProperty]
	float dayViewCornerRadius;

	[ObservableProperty]
	Style daysLabelStyle = DefaultStyles.DefaultLabelStyle;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundFullEventColor)
	)]
	bool eventDayBackgroundColorIsActive = false;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundFullEventColor)
	)]
	Color eventDayBackgroundColor;

	[ObservableProperty]
	Style eventIndicatorDotStyle = DefaultStyles.DefaultEventIndicatorDotStyle;

	[ObservableProperty]
	Style eventIndicatorTextContainerStyle = DefaultStyles.DefaultEventIndicatorTextContainerStyle;

	[ObservableProperty]
	Style eventIndicatorTextStyle = DefaultStyles.DefaultEventIndicatorTextStyle;

	[ObservableProperty]
	Style eventIndicatorImageStyle = DefaultStyles.DefaultEventIndicatorImageStyle;

	[ObservableProperty]
	ICommand dayTappedCommand;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundColor), 
		nameof(OutlineColor), 
		nameof(BackgroundFullEventColor)
	)]
	[NotifyPropertyChangedFor(
		nameof(BackgroundFullEventColor)
	)] 
	bool hasEvents;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(TextColor), 
		nameof(IsVisible), 
		nameof(IsControlVisible)
	)]
	bool isThisMonth;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(TextColor),
		nameof(BackgroundColor),
		nameof(OutlineColor),
		nameof(BackgroundFullEventColor)
	)]
	bool isSelected;

	[ObservableProperty]
	bool allowDeselect;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsVisible))]
	bool otherMonthIsVisible;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsControlVisible))]
	bool otherMonthWeekIsVisible;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	bool isDisabled;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color selectedTextColor = Colors.White;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color selectedTodayTextColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color otherMonthColor = Colors.Silver;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color otherMonthSelectedColor = Colors.Gray;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color weekendDayColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color deselectedTextColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	Color selectedBackgroundColor = Color.FromArgb("#2196F3");

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	Color deselectedBackgroundColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(
		nameof(BackgroundColor),
		nameof(DayIndicatorViewVerticalOptions)
	)]
	EventIndicatorPlacementType eventIndicatorType = EventIndicatorPlacementType.Bottom;

	[ObservableProperty]
	List<EventIndicatorModel> eventIndicators;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(OutlineColor))]
	Color todayOutlineColor = Color.FromArgb("#FF4081");

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(TextColor))]
	Color todayTextColor = Colors.Transparent;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(BackgroundColor))]
	Color todayFillColor = Colors.Transparent;

	[ObservableProperty]
	Color disabledColor = Color.FromArgb("#ECECEC");

	public Thickness DayIndicatorViewMargin
	{
		get
		{
			double minMargin = 2.0;

			return new Thickness(
				Math.Max(DayViewBorderMargin.Left / 4, minMargin),
				Math.Max(DayViewBorderMargin.Top / 4, minMargin),
				Math.Max(DayViewBorderMargin.Right / 4, minMargin),
				Math.Max(DayViewBorderMargin.Bottom / 4, minMargin)
			);
		}
	}

	public LayoutOptions DayIndicatorViewVerticalOptions =>  EventIndicatorType == EventIndicatorPlacementType.Top ? LayoutOptions.Start : LayoutOptions.End;

	public Color BackgroundFullEventColor => HasEvents && EventDayBackgroundColorIsActive ? EventDayBackgroundColor : Colors.Transparent;

	public Color OutlineColor => IsToday && !IsSelected ? TodayOutlineColor : Colors.Transparent;

	public Color BackgroundColor
	{
		get
		{
			if (!IsVisible || IsDisabled)
			{
				return DeselectedBackgroundColor;
			}

			return (IsSelected, IsToday) switch
			{
				(true, _) => SelectedBackgroundColor,
				(false, true) => TodayFillColor,
				(_, _) => DeselectedBackgroundColor
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
				(false, true, _, true, true, _)	=> SelectedTodayTextColor == Colors.Transparent ? SelectedTextColor : SelectedTodayTextColor,
				(false, true, _, true, false, _) => SelectedTextColor,
				(false, true, _, false, _, _) => OtherMonthSelectedColor,
				(false, false, _, false, _, _) => OtherMonthColor,
				(false, false, _, true, true, _) => TodayTextColor == Colors.Transparent ? DeselectedTextColor : TodayTextColor,
				(false, _, _, _, _, true) => WeekendDayColor,
				_ => DeselectedTextColor
			};
		}
	}

	public bool IsVisible => IsThisMonth || OtherMonthIsVisible;

	public bool IsControlVisible => IsThisMonth || OtherMonthWeekIsVisible;

	bool IsToday => Date.Date == DateTime.Today;

	public bool IsWeekend => (Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday) && WeekendDayColor != Colors.Transparent;
}
