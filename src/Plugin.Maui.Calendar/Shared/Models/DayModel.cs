using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;

namespace Plugin.Maui.Calendar.Models;

internal class DayModel : BindableBase<DayModel>
{
    public DateTime Date
    {
        get => GetProperty<DateTime>();
        set => SetProperty(value)
                .Notify(nameof(BackgroundColor),
                        nameof(OutlineColor));
    }

    public double DayViewSize
    {
        get => GetProperty<double>();
        set => SetProperty(value);
    }
    [TypeConverter(typeof(FontSizeConverter))]

    public double FontSize
    {
        get => GetProperty<double>(14);
        set => SetProperty(value);
    }

    public float DayViewCornerRadius
    {
        get => GetProperty<float>();
        set => SetProperty(value);
    }

    public Style DaysLabelStyle
    {
        get => GetProperty(DefaultStyles.DefaultLabelStyle);
        set => SetProperty(value);
    }    

    public ICommand DayTappedCommand
    {
        get => GetProperty<ICommand>();
        set => SetProperty(value);
    }

    public bool HasEvents
    {
        get => GetProperty<bool>();
        set => SetProperty(value)
                .Notify(nameof(IsEventDotVisible),
                        nameof(BackgroundEventIndicator),
                        nameof(BackgroundFullEventColor));
    }

    public bool IsThisMonth
    {
        get => GetProperty<bool>();
        set => SetProperty(value)
                .Notify(nameof(TextColor),
                        nameof(IsVisible));
    }

    public bool IsSelected
    {
        get => GetProperty<bool>();
        set => SetProperty(value)
                .Notify(nameof(TextColor),
                        nameof(BackgroundColor),
                        nameof(OutlineColor),
                        nameof(EventColor),
                        nameof(BackgroundFullEventColor));
    }

    public bool OtherMonthIsVisible
    {
        get => GetProperty(true);
        set => SetProperty(value)
                .Notify(nameof(IsVisible));
    }

    public bool IsDisabled
    {
        get => GetProperty<bool>();
        set => SetProperty(value)
                .Notify(nameof(TextColor));
    }

    public Color SelectedTextColor
    {
        get => GetProperty(Colors.White);
        set => SetProperty(value)
                .Notify(nameof(TextColor));
    }

    public Color SelectedTodayTextColor 
    {
        get => GetProperty(Colors.Transparent);
        set => SetProperty(value)
                .Notify(nameof(TextColor));
    }

    public Color OtherMonthColor
    {
        get => GetProperty(Colors.Silver);
        set => SetProperty(value)
                .Notify(nameof(TextColor));
    }

    public Color DeselectedTextColor
    {
        get => GetProperty(Colors.Transparent);
        set => SetProperty(value)
                .Notify(nameof(TextColor));
    }

    public Color SelectedBackgroundColor
    {
        get => GetProperty(Color.FromArgb("#2196F3"));
        set => SetProperty(value)
                .Notify(nameof(BackgroundColor));
    }

    public Color DeselectedBackgroundColor
    {
        get => GetProperty(Colors.Transparent);
        set => SetProperty(value)
                .Notify(nameof(BackgroundColor));
    }

    public EventIndicatorType EventIndicatorType
    {
        get => GetProperty(EventIndicatorType.BottomDot);
        set => SetProperty(value)
                .Notify(nameof(IsEventDotVisible),
                        nameof(BackgroundEventIndicator),
                        nameof(BackgroundColor));
    }

    public Color EventIndicatorColor
    {
        get => GetProperty(Color.FromArgb("#FF4081"));
        set => SetProperty(value)
                .Notify(nameof(EventColor),
                        nameof(BackgroundColor),
                        nameof(BackgroundFullEventColor));
    }

    public Color EventIndicatorSelectedColor
    {
        get => GetProperty(SelectedBackgroundColor);
        set => SetProperty(value)
                .Notify(nameof(EventColor),
                        nameof(BackgroundColor),
                        nameof(BackgroundFullEventColor));
    }

    public Color EventIndicatorTextColor
    {
        get => GetProperty(DeselectedTextColor);
        set => SetProperty(value);
    }

    public Color EventIndicatorSelectedTextColor
    {
        get => GetProperty(SelectedTextColor);
        set => SetProperty(value);
    }

    public Color TodayOutlineColor
    {
        get => GetProperty(Color.FromArgb("#FF4081"));
        set => SetProperty(value)
                .Notify(nameof(OutlineColor));
    }

    public Color TodayTextColor 
    { 
        get => GetProperty(Colors.Transparent); 
        set => SetProperty(value)
                .Notify(nameof(TextColor)); 
    }

    public Color TodayFillColor
    {
        get => GetProperty(Colors.Transparent);
        set => SetProperty(value)
                .Notify(nameof(BackgroundColor));
    }

    public Color DisabledColor
    {
        get => GetProperty(Color.FromArgb("#ECECEC"));
        set => SetProperty(value);
    }

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

    public Color TextColor
    {
        get
        {
            if (!IsVisible) return OtherMonthColor;

            return (IsDisabled, IsSelected, HasEvents, IsThisMonth, IsToday) switch
            {
                (true, _, _, _, _) => DisabledColor,
                (false, true, false, true, true) => SelectedTodayTextColor == Colors.Transparent? SelectedTextColor : SelectedTodayTextColor,
                (false, true, false, true, false) => SelectedTextColor,
                (false, true, true, true, _) => EventIndicatorSelectedTextColor,
                (false, false, true, true, _) => EventIndicatorTextColor,
                (false, false, _, false, _) => OtherMonthColor,
                (false, false, false, true, true) => TodayTextColor == Colors.Transparent? DeselectedTextColor : TodayTextColor,
                (false, false, false, true, false) => DeselectedTextColor,
                (_, _, _, _, _) => Colors.Black
            };
        }
    }

    public bool IsVisible => IsThisMonth || OtherMonthIsVisible;

    private bool IsToday
        => Date.Date == DateTime.Today;
}
