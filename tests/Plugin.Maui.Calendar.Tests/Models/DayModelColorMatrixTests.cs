using FluentAssertions;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

/// <summary>
/// Verifies every rule of <see cref="DayModel.TextColor"/> and <see cref="DayModel.BackgroundColor"/>:
/// each state of a day (hidden, disabled, selected, with events, of another month, today, weekend)
/// picks its own color. Every color property gets a distinct color, so a test names the one it expects.
/// </summary>
public class DayModelColorMatrixTests
{
    static readonly DateTime Today = new(2025, 5, 14); // a Wednesday
    static readonly DateTime Saturday = new(2025, 5, 17);
    static readonly DateTime Monday = new(2025, 5, 12);

    static readonly Color otherMonth = Color.FromArgb("#010101");
    static readonly Color otherMonthSelected = Color.FromArgb("#020202");
    static readonly Color disabled = Color.FromArgb("#030303");
    static readonly Color selectedText = Color.FromArgb("#040404");
    static readonly Color selectedTodayText = Color.FromArgb("#050505");
    static readonly Color eventSelectedText = Color.FromArgb("#060606");
    static readonly Color eventText = Color.FromArgb("#070707");
    static readonly Color todayText = Color.FromArgb("#080808");
    static readonly Color deselectedText = Color.FromArgb("#090909");
    static readonly Color weekend = Color.FromArgb("#0A0A0A");
    static readonly Color eventIndicator = Color.FromArgb("#0B0B0B");
    static readonly Color eventIndicatorSelected = Color.FromArgb("#0C0C0C");
    static readonly Color selectedBackground = Color.FromArgb("#0D0D0D");
    static readonly Color todayFill = Color.FromArgb("#0E0E0E");
    static readonly Color deselectedBackground = Color.FromArgb("#0F0F0F");

    static DayModel Day(DateTime date, bool thisMonth = true, bool selected = false, bool hasEvents = false, bool disabled = false)
    {
        var day = new DayModel
        {
            Today = Today,
            OtherMonthColor = otherMonth,
            OtherMonthSelectedColor = otherMonthSelected,
            DisabledColor = DayModelColorMatrixTests.disabled,
            SelectedTextColor = selectedText,
            SelectedTodayTextColor = selectedTodayText,
            EventIndicatorSelectedTextColor = eventSelectedText,
            EventIndicatorTextColor = eventText,
            TodayTextColor = todayText,
            DeselectedTextColor = deselectedText,
            WeekendDayColor = weekend,
            EventIndicatorColor = eventIndicator,
            EventIndicatorSelectedColor = eventIndicatorSelected,
            SelectedBackgroundColor = selectedBackground,
            TodayFillColor = todayFill,
            DeselectedBackgroundColor = deselectedBackground,
            OtherMonthIsVisible = true,
        };
        day.Date = date;
        day.IsThisMonth = thisMonth;
        day.IsSelected = selected;
        day.HasEvents = hasEvents;
        day.IsDisabled = disabled;
        return day;
    }

    // ── TextColor ────────────────────────────────────────────────────────────

    [Fact]
    public void TextColor_HiddenDay_IsTheOtherMonthColor()
    {
        var day = Day(Monday, thisMonth: false, selected: true);
        day.OtherMonthIsVisible = false;

        day.TextColor.Should().Be(otherMonth);
    }

    [Fact]
    public void TextColor_DisabledDay_IsTheDisabledColorWhateverItsState() =>
        Day(Today, selected: true, hasEvents: true, disabled: true).TextColor.Should().Be(disabled);

    [Fact]
    public void TextColor_SelectedToday_IsTheSelectedTodayColorOrTheSelectedColorWhenTransparent()
    {
        var day = Day(Today, selected: true);
        day.TextColor.Should().Be(selectedTodayText);

        day.SelectedTodayTextColor = Colors.Transparent;
        day.TextColor.Should().Be(selectedText);
    }

    [Fact]
    public void TextColor_SelectedDay_IsTheSelectedColor() =>
        Day(Monday, selected: true).TextColor.Should().Be(selectedText);

    [Fact]
    public void TextColor_SelectedDayWithEvents_IsTheSelectedEventColor() =>
        Day(Monday, selected: true, hasEvents: true).TextColor.Should().Be(eventSelectedText);

    [Fact]
    public void TextColor_DayWithEvents_IsTheEventColor() =>
        Day(Saturday, hasEvents: true).TextColor.Should().Be(eventText);

    [Fact]
    public void TextColor_DayOfAnotherMonth_IsTheOtherMonthColorEvenWithEvents() =>
        Day(Monday, thisMonth: false, hasEvents: true).TextColor.Should().Be(otherMonth);

    [Fact]
    public void TextColor_SelectedDayOfAnotherMonth_IsTheOtherMonthSelectedColor() =>
        Day(Monday, thisMonth: false, selected: true).TextColor.Should().Be(otherMonthSelected);

    [Fact]
    public void TextColor_Today_IsTheTodayColorOrTheDeselectedColorWhenTransparent()
    {
        var day = Day(Today);
        day.TextColor.Should().Be(todayText);

        day.TodayTextColor = Colors.Transparent;
        day.TextColor.Should().Be(deselectedText);
    }

    [Fact]
    public void TextColor_Weekend_IsTheWeekendColor() =>
        Day(Saturday).TextColor.Should().Be(weekend);

    [Fact]
    public void TextColor_OrdinaryDay_IsTheDeselectedColor() =>
        Day(Monday).TextColor.Should().Be(deselectedText);

    // ── BackgroundColor ──────────────────────────────────────────────────────

    [Fact]
    public void BackgroundColor_HiddenOrDisabledDay_IsTheDeselectedBackground()
    {
        var hidden = Day(Monday, thisMonth: false, selected: true);
        hidden.OtherMonthIsVisible = false;

        hidden.BackgroundColor.Should().Be(deselectedBackground);
        Day(Monday, selected: true, disabled: true).BackgroundColor.Should().Be(deselectedBackground);
    }

    [Fact]
    public void BackgroundColor_BackgroundEventIndicator_IsTheEventColorSelectedOrNot()
    {
        var day = Day(Monday, hasEvents: true);
        day.EventIndicatorType = EventIndicatorType.Background;

        day.BackgroundColor.Should().Be(eventIndicator);

        day.IsSelected = true;
        day.BackgroundColor.Should().Be(eventIndicatorSelected);
    }

    [Fact]
    public void BackgroundColor_SelectedDay_IsTheSelectedBackground() =>
        Day(Monday, selected: true, hasEvents: true).BackgroundColor.Should().Be(selectedBackground);

    [Fact]
    public void BackgroundColor_Today_IsTheTodayFill() =>
        Day(Today).BackgroundColor.Should().Be(todayFill);

    [Fact]
    public void BackgroundColor_OrdinaryDay_IsTheDeselectedBackground() =>
        Day(Monday).BackgroundColor.Should().Be(deselectedBackground);

    // ── Other state ──────────────────────────────────────────────────────────

    [Fact]
    public void BackgroundFullEventColor_IsTheEventColorOnlyForTheFullBackgroundIndicator()
    {
        var day = Day(Monday, hasEvents: true);
        day.BackgroundFullEventColor.Should().Be(Colors.Transparent);

        day.EventIndicatorType = EventIndicatorType.BackgroundFull;
        day.BackgroundFullEventColor.Should().Be(eventIndicator);

        day.HasEvents = false;
        day.BackgroundFullEventColor.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void OutlineColor_IsTheTodayOutlineOnlyForTodayWhenNotSelected()
    {
        var day = Day(Today);
        day.OutlineColor.Should().Be(day.TodayOutlineColor);

        day.IsSelected = true;
        day.OutlineColor.Should().Be(Colors.Transparent);

        Day(Monday).OutlineColor.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void IsControlVisible_HidesOnlyTheWeeksOfOtherMonthsThatAreHidden()
    {
        var day = Day(Monday, thisMonth: false);
        day.OtherMonthWeekIsVisible = true;
        day.IsControlVisible.Should().BeTrue();

        day.OtherMonthWeekIsVisible = false;
        day.IsControlVisible.Should().BeFalse();

        day.IsThisMonth = true;
        day.IsControlVisible.Should().BeTrue();
    }
}
