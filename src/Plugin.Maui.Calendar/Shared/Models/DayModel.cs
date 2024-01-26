using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Styles;

namespace Plugin.Maui.Calendar.Models;

internal partial class DayModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    [NotifyPropertyChangedFor(nameof(OutlineColor))]
    DateTime date;

    [ObservableProperty]
    double dayViewSize;

    [ObservableProperty]
    float dayViewCornerRadius;

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
    [NotifyPropertyChangedFor(nameof(BackgroundColor), nameof(OutlineColor), nameof(EventColor), nameof(BackgroundFullEventColor))]
    bool isSelected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    bool otherMonthIsVisible;

    [ObservableProperty]
    bool isDisabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    Color selectedBackgroundColor = Color.FromArgb("#2196F3");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    Color deselectedBackgroundColor = Colors.Transparent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEventDotVisible), nameof(BackgroundEventIndicator), nameof(BackgroundColor))]
    EventIndicatorType eventIndicatorType = EventIndicatorType.BottomDot;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EventColor), nameof(BackgroundColor), nameof(BackgroundFullEventColor))]
    Color eventIndicatorColor = Color.FromArgb("#FF4081");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EventColor), nameof(BackgroundColor), nameof(BackgroundFullEventColor))]
    Color eventIndicatorSelectedColor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OutlineColor))]
    Color todayOutlineColor = Color.FromArgb("#FF4081");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    Color todayFillColor = Colors.Transparent;


    public bool IsEventDotVisible => HasEvents && (EventIndicatorType == EventIndicatorType.BottomDot || EventIndicatorType == EventIndicatorType.TopDot);

    public FlexDirection EventLayoutDirection => (HasEvents && EventIndicatorType == EventIndicatorType.TopDot) ? FlexDirection.ColumnReverse : FlexDirection.Column;

    public bool BackgroundEventIndicator => HasEvents && EventIndicatorType == EventIndicatorType.Background;

    public Color BackgroundFullEventColor => HasEvents && EventIndicatorType == EventIndicatorType.BackgroundFull
                                           ? EventColor
                                           : Colors.Transparent;

    public Color EventColor => IsSelected
                             ? EventIndicatorSelectedColor
                             : EventIndicatorColor;

    public Color OutlineColor => IsToday && !IsSelected
                               ? TodayOutlineColor
                               : Colors.Transparent;

    public Color BackgroundColor
    {
        get
        {
            if (!IsVisible) return DeselectedBackgroundColor;

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

    public bool IsVisible => IsThisMonth || OtherMonthIsVisible;

    public bool IsToday => Date.Date == DateTime.Today;

    public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;
}
