using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Models;

internal partial class DayModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundStyle))]
    DateTime date;

    [ObservableProperty]
    Style daysLabelStyle = DefaultStyles.DefaultLabelStyle;

    [ObservableProperty]
    ICommand dayTappedCommand;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEventDotVisible), nameof(BackgroundEventIndicator), nameof(BackgroundFullEventColor))]
    bool hasEvents;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    bool isThisMonth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundStyle), nameof(EventStyle), nameof(BackgroundFullEventColor))]
    bool isSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    bool otherMonthIsVisible;

    [ObservableProperty]
    bool isDisabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundStyle))]
    Style selectedDayViewBorderStyle = DefaultStyles.DefaultSelectedDayViewBorderStyle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundStyle))]
    Style deselectedDayViewBorderStyle = DefaultStyles.DefaultDeselectedDayViewBorderStyle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEventDotVisible), nameof(BackgroundEventIndicator), nameof(BackgroundStyle))]
    EventIndicatorType eventIndicatorType = EventIndicatorType.BottomDot;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EventStyle), nameof(BackgroundStyle), nameof(BackgroundFullEventColor))]
    Style eventIndicatorStyle = DefaultStyles.DefaultEventIndicatorStyle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EventStyle), nameof(BackgroundStyle), nameof(BackgroundFullEventColor))]
    Style eventIndicatorSelectedStyle = DefaultStyles.DefaultEventIndicatorSelectedStyle;

    [ObservableProperty]
    Style todayDayViewBorderStyle = DefaultStyles.DefaultTodayDayViewBorderStyle;

    public bool IsEventDotVisible => HasEvents && (EventIndicatorType == EventIndicatorType.BottomDot || EventIndicatorType == EventIndicatorType.TopDot);

    public FlexDirection EventLayoutDirection => (HasEvents && EventIndicatorType == EventIndicatorType.TopDot) ? FlexDirection.ColumnReverse : FlexDirection.Column;

    public bool BackgroundEventIndicator => HasEvents && EventIndicatorType == EventIndicatorType.Background;

    public Color BackgroundFullEventColor => HasEvents && EventIndicatorType == EventIndicatorType.BackgroundFull
                                           ? Color.FromArgb("#FF4081") //Todo: replace later with style //EventStyle
                                           : Colors.Transparent;

    public Style EventStyle => IsSelected
                             ? EventIndicatorSelectedStyle
                             : EventIndicatorStyle;


    public Style BackgroundStyle
    {
        get
        {
            if (!IsVisible) return DeselectedDayViewBorderStyle;

            return (BackgroundEventIndicator, IsSelected, IsToday) switch
            {
                (true, false, _) => EventIndicatorStyle,
                (true, true, _) => EventIndicatorSelectedStyle,
                (false, true, _) => SelectedDayViewBorderStyle,
                (false, false, true) => TodayDayViewBorderStyle,
                (_, _, _) => DeselectedDayViewBorderStyle
            };
        }
    }

    public bool IsVisible => IsThisMonth || OtherMonthIsVisible;

    public bool IsToday => Date.Date == DateTime.Today;

    public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;
}
