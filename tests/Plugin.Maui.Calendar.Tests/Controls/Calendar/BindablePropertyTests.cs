using System.Globalization;
using System.Reflection;
using System.Windows.Input;
using FluentAssertions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies every public bindable property of the calendars: it keeps a value that is set (so a
/// binding can use it), and every calendar-wide day property reaches the day cells when it changes
/// at runtime.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class BindablePropertyTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    /// <summary>Every public settable property of <paramref name="type"/> that has a bindable property.</summary>
    static IEnumerable<PropertyInfo> BindableProperties(Type type) =>
        type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(property => property.SetMethod?.IsPublic == true
                && type.GetField(property.Name + "Property", BindingFlags.Static | BindingFlags.Public) is not null);

    public static TheoryData<string> CalendarProperties =>
        [.. BindableProperties(typeof(CalendarControl)).Select(property => property.Name)];

    public static TheoryData<string> RangeCalendarProperties =>
        [.. BindableProperties(typeof(RangeSelectionCalendar)).Select(property => property.Name)];

    /// <summary>A value of <paramref name="type"/> that differs from the property's default.</summary>
    static object SampleValue(Type type, object? current) => type switch
    {
        _ when type == typeof(bool) => !(bool)current!,
        _ when type == typeof(int) => 7,
        _ when type == typeof(double) => 33.5,
        _ when type == typeof(float) => 5.5f,
        _ when type == typeof(string) => "d/M",
        _ when type == typeof(DateTime) => new DateTime(2025, 7, 7),
        _ when type == typeof(DateTime?) => new DateTime(2025, 7, 7),
        _ when type == typeof(Color) => Color.FromArgb("#123456"),
        _ when type == typeof(Thickness) => new Thickness(1, 2, 3, 4),
        _ when type == typeof(Style) => new Style(typeof(Label)),
        _ when type == typeof(DataTemplate) => new DataTemplate(() => new Label()),
        _ when type == typeof(ICommand) => new Command(() => { }),
        _ when type == typeof(CultureInfo) => new CultureInfo("uk-UA"),
        _ when type == typeof(TimeProvider) => new Microsoft.Extensions.Time.Testing.FakeTimeProvider(),
        _ when type == typeof(EventCollection) => new EventCollection(),
        _ when type == typeof(List<DateTime>) => new List<DateTime> { new(2025, 5, 3) },
        _ when type == typeof(System.Collections.ObjectModel.ObservableCollection<DateTime>) => new System.Collections.ObjectModel.ObservableCollection<DateTime> { new(2025, 5, 3) },
        _ when type == typeof(System.Collections.ICollection) => new List<object> { "event" },
        _ when type == typeof(DayOfWeek) => DayOfWeek.Wednesday,
        _ when type.IsEnum => Enum.GetValues(type).Cast<object>().First(value => !value.Equals(current)),
        _ => throw new NotSupportedException($"No sample value for {type}."),
    };

    static void ExpectRoundTrip(CalendarControl calendar, string propertyName)
    {
        var property = calendar.GetType().GetProperty(propertyName)!;
        var value = SampleValue(property.PropertyType, property.GetValue(calendar));

        property.SetValue(calendar, value);

        // A single-selection calendar replaces SelectedDates with a new collection of its first date.
        property.GetValue(calendar).Should().BeEquivalentTo(value, $"{propertyName} must keep the value it was given");
    }

    [Theory]
    [MemberData(nameof(CalendarProperties))]
    public void CalendarProperty_KeepsTheValueItIsGiven(string propertyName) =>
        ExpectRoundTrip(new TestCalendar { ShownDate = May15 }, propertyName);

    [Theory]
    [MemberData(nameof(RangeCalendarProperties))]
    public void RangeCalendarProperty_KeepsTheValueItIsGiven(string propertyName) =>
        ExpectRoundTrip(new TestRangeSelectionCalendar { ShownDate = May15 }, propertyName);

    [Fact]
    public void CalendarProperties_AreDiscovered()
    {
        // Guards the discovery above: a renamed field would silently drop properties from the theory.
        CalendarProperties.Count.Should().BeGreaterThan(80);
    }

    // ── Calendar-wide day properties reach the cells ─────────────────────────

    public static TheoryData<string, string> DayProperties => new()
    {
        { nameof(CalendarControl.DeselectedDayTextColor), nameof(DayModel.DeselectedTextColor) },
        { nameof(CalendarControl.SelectedDayTextColor), nameof(DayModel.SelectedTextColor) },
        { nameof(CalendarControl.SelectedTodayTextColor), nameof(DayModel.SelectedTodayTextColor) },
        { nameof(CalendarControl.OtherMonthDayColor), nameof(DayModel.OtherMonthColor) },
        { nameof(CalendarControl.OtherMonthSelectedDayColor), nameof(DayModel.OtherMonthSelectedColor) },
        { nameof(CalendarControl.WeekendDayColor), nameof(DayModel.WeekendDayColor) },
        { nameof(CalendarControl.SelectedDayBackgroundColor), nameof(DayModel.SelectedBackgroundColor) },
        { nameof(CalendarControl.TodayOutlineColor), nameof(DayModel.TodayOutlineColor) },
        { nameof(CalendarControl.TodayTextColor), nameof(DayModel.TodayTextColor) },
        { nameof(CalendarControl.TodayFillColor), nameof(DayModel.TodayFillColor) },
        { nameof(CalendarControl.DisabledDayColor), nameof(DayModel.DisabledColor) },
        { nameof(CalendarControl.DayViewSize), nameof(DayModel.DayViewSize) },
        { nameof(CalendarControl.DayViewBorderMargin), nameof(DayModel.DayViewBorderMargin) },
        { nameof(CalendarControl.DayViewCornerRadius), nameof(DayModel.DayViewCornerRadius) },
        { nameof(CalendarControl.DaysLabelStyle), nameof(DayModel.DaysLabelStyle) },
        { nameof(CalendarControl.DayTappedCommand), nameof(DayModel.DayTappedCommand) },
        { nameof(CalendarControl.EventIndicatorType), nameof(DayModel.EventIndicatorType) },
        { nameof(CalendarControl.AllowDeselecting), nameof(DayModel.AllowDeselect) },
    };

    [Theory]
    [MemberData(nameof(DayProperties))]
    public void DayProperty_ChangedAtRuntime_ReachesEveryCell(string calendarProperty, string dayProperty)
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var property = typeof(CalendarControl).GetProperty(calendarProperty)!;
        var value = SampleValue(property.PropertyType, property.GetValue(calendar));

        property.SetValue(calendar, value);

        calendar.Days().Should().AllSatisfy(day =>
            typeof(DayModel).GetProperty(dayProperty)!.GetValue(day).Should().Be(value));
    }

    [Fact]
    public void DayViewHeight_ChangedAtRuntime_ReachesEveryCell()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.DayViewHeight = 64;

        calendar.Days().Should().AllSatisfy(day => day.DayViewHeight.Should().Be(64));
    }
}
