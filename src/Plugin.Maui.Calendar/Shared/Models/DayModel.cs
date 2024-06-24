using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;

namespace Plugin.Maui.Calendar.Models;

internal partial class DayModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    [NotifyPropertyChangedFor(nameof(OutlineColor))]
    DateTime date;

    [ObservableProperty]
    double dayViewSize;

    [TypeConverter(typeof(FontSizeConverter))]
    [ObservableProperty]
    double fontSize = 14;

    [ObservableProperty]
    float dayViewCornerRadius;

    [ObservableProperty]
    Style daysLabelStyle = DefaultStyles.DefaultLabelStyle;

    [ObservableProperty]
    ICommand dayTappedCommand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsEventDotVisible),
        nameof(BackgroundEventIndicator),
        nameof(BackgroundFullEventColor)
    )]
    bool hasEvents;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextColor), nameof(IsVisible))]
    bool isThisMonth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(TextColor),
        nameof(BackgroundColor),
        nameof(OutlineColor),
        nameof(EventColor),
        nameof(BackgroundFullEventColor)
    )]
    bool isSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    bool otherMonthIsVisible;

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
        nameof(IsEventDotVisible),
        nameof(BackgroundEventIndicator),
        nameof(BackgroundColor)
    )]
    EventIndicatorType eventIndicatorType = EventIndicatorType.BottomDot;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(EventColor),
        nameof(BackgroundColor),
        nameof(BackgroundFullEventColor)
    )]
    Color eventIndicatorColor = Color.FromArgb("#FF4081");

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(EventColor),
        nameof(BackgroundColor),
        nameof(BackgroundFullEventColor)
    )]
    Color eventIndicatorSelectedColor;

    [ObservableProperty]
    Color eventIndicatorTextColor;

    [ObservableProperty]
    Color eventIndicatorSelectedTextColor;

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

    public bool IsEventDotVisible =>
        HasEvents
        && (
            EventIndicatorType == EventIndicatorType.BottomDot
            || EventIndicatorType == EventIndicatorType.TopDot
        );

    public FlexDirection EventLayoutDirection =>
        (HasEvents && EventIndicatorType == EventIndicatorType.TopDot)
            ? FlexDirection.ColumnReverse
            : FlexDirection.Column;

    public bool BackgroundEventIndicator =>
        HasEvents && EventIndicatorType == EventIndicatorType.Background;

    public Color BackgroundFullEventColor =>
        HasEvents && EventIndicatorType == EventIndicatorType.BackgroundFull
            ? EventColor
            : Colors.Transparent;

    public Color EventColor => IsSelected ? EventIndicatorSelectedColor : EventIndicatorColor;

    public Color OutlineColor => IsToday && !IsSelected ? TodayOutlineColor : Colors.Transparent;

    public Color BackgroundColor
    {
        get
        {
            if (!IsVisible || IsDisabled)
                return DeselectedBackgroundColor;

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
                return OtherMonthColor;

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

    private bool IsToday => Date.Date == DateTime.Today;

    public bool IsWeekend =>
        (Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday)
        && WeekendDayColor != Colors.Transparent;
}
