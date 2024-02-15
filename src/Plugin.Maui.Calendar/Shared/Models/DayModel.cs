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
    [NotifyPropertyChangedFor(nameof(BackgroundColor), nameof(OutlineColor), nameof(EventStyle), nameof(BackgroundFullEventColor))]
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
    [NotifyPropertyChangedFor(nameof(EventStyle), nameof(BackgroundColor), nameof(BackgroundFullEventColor))]
    Style eventIndicatorStyle = DefaultStyles.DefaultEventIndicatorStyle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EventStyle), nameof(BackgroundColor), nameof(BackgroundFullEventColor))]
    Style eventIndicatorSelectedStyle = DefaultStyles.DefaultEventIndicatorSelectedStyle;

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
                                           ? Color.FromArgb("#FF4081") //Todo: replace later with style //EventStyle
                                           : Colors.Transparent;

    public Style EventStyle => IsSelected
                             ? EventIndicatorSelectedStyle
                             : EventIndicatorStyle;

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
                (true, false, _) => Color.FromArgb("#FF4081"), //Todo: replace later with style //EventIndicatorStyle,
                (true, true, _) => Colors.Black,              //Todo: replace later with style  //EventIndicatorSelectedStyle,
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
