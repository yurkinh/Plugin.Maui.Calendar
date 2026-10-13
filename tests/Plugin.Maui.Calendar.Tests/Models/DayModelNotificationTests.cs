using System.Reflection;
using FluentAssertions;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

/// <summary>
/// Verifies the PropertyChanged notifications of every settable member of <see cref="DayModel"/>: a
/// new value raises the member itself and then every value computed from it, in that order, and the
/// same value again raises nothing. Day cells and DayViewTemplates bind to these values, so a missing
/// notification leaves a cell showing a stale color.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class DayModelNotificationTests
{
    public static TheoryData<string, string[]> Notifications => new()
    {
        { nameof(DayModel.Date), [nameof(DayModel.Date), nameof(DayModel.BackgroundColor), nameof(DayModel.OutlineColor), nameof(DayModel.TextColor), nameof(DayModel.IsToday), nameof(DayModel.IsWeekend)] },
        { nameof(DayModel.Day), [nameof(DayModel.Day)] },
        { nameof(DayModel.DayViewBorderMargin), [nameof(DayModel.DayViewBorderMargin)] },
        { nameof(DayModel.DayViewSize), [nameof(DayModel.DayViewSize)] },
        { nameof(DayModel.DayViewHeight), [nameof(DayModel.DayViewHeight)] },
        { nameof(DayModel.DayViewCornerRadius), [nameof(DayModel.DayViewCornerRadius)] },
        { nameof(DayModel.DaysLabelStyle), [nameof(DayModel.DaysLabelStyle)] },
        { nameof(DayModel.DayTappedCommand), [nameof(DayModel.DayTappedCommand)] },
        { nameof(DayModel.HasEvents), [nameof(DayModel.HasEvents), nameof(DayModel.TextColor), nameof(DayModel.BackgroundColor), nameof(DayModel.BackgroundEventIndicator), nameof(DayModel.BackgroundFullEventColor)] },
        { nameof(DayModel.EventCount), [nameof(DayModel.EventCount)] },
        { nameof(DayModel.Events), [nameof(DayModel.Events)] },
        { nameof(DayModel.IsRangeStart), [nameof(DayModel.IsRangeStart)] },
        { nameof(DayModel.IsRangeEnd), [nameof(DayModel.IsRangeEnd)] },
        { nameof(DayModel.IsThisMonth), [nameof(DayModel.IsThisMonth), nameof(DayModel.TextColor), nameof(DayModel.IsVisible), nameof(DayModel.IsControlVisible), nameof(DayModel.BackgroundFullEventColor)] },
        { nameof(DayModel.IsSelected), [nameof(DayModel.IsSelected), nameof(DayModel.TextColor), nameof(DayModel.BackgroundColor), nameof(DayModel.OutlineColor), nameof(DayModel.BackgroundFullEventColor)] },
        { nameof(DayModel.AllowDeselect), [nameof(DayModel.AllowDeselect)] },
        { nameof(DayModel.OtherMonthIsVisible), [nameof(DayModel.OtherMonthIsVisible), nameof(DayModel.IsVisible), nameof(DayModel.BackgroundFullEventColor)] },
        { nameof(DayModel.OtherMonthWeekIsVisible), [nameof(DayModel.OtherMonthWeekIsVisible), nameof(DayModel.IsControlVisible)] },
        { nameof(DayModel.IsDisabled), [nameof(DayModel.IsDisabled), nameof(DayModel.TextColor)] },
        { nameof(DayModel.SelectedTextColor), [nameof(DayModel.SelectedTextColor), nameof(DayModel.TextColor)] },
        { nameof(DayModel.SelectedTodayTextColor), [nameof(DayModel.SelectedTodayTextColor), nameof(DayModel.TextColor)] },
        { nameof(DayModel.OtherMonthColor), [nameof(DayModel.OtherMonthColor), nameof(DayModel.TextColor)] },
        { nameof(DayModel.OtherMonthSelectedColor), [nameof(DayModel.OtherMonthSelectedColor), nameof(DayModel.TextColor)] },
        { nameof(DayModel.WeekendDayColor), [nameof(DayModel.WeekendDayColor), nameof(DayModel.TextColor)] },
        { nameof(DayModel.DeselectedTextColor), [nameof(DayModel.DeselectedTextColor), nameof(DayModel.TextColor)] },
        { nameof(DayModel.SelectedBackgroundColor), [nameof(DayModel.SelectedBackgroundColor), nameof(DayModel.BackgroundColor)] },
        { nameof(DayModel.DeselectedBackgroundColor), [nameof(DayModel.DeselectedBackgroundColor), nameof(DayModel.BackgroundColor)] },
        { nameof(DayModel.EventIndicatorType), [nameof(DayModel.EventIndicatorType), nameof(DayModel.BackgroundEventIndicator), nameof(DayModel.BackgroundColor), nameof(DayModel.BackgroundFullEventColor), nameof(DayModel.EventLayoutDirection)] },
        { nameof(DayModel.EventIndicatorColor), [nameof(DayModel.EventIndicatorColor), nameof(DayModel.BackgroundColor), nameof(DayModel.BackgroundFullEventColor)] },
        { nameof(DayModel.EventColors), [nameof(DayModel.EventColors)] },
        { nameof(DayModel.EventIndicatorSelectedColor), [nameof(DayModel.EventIndicatorSelectedColor), nameof(DayModel.BackgroundColor), nameof(DayModel.BackgroundFullEventColor)] },
        { nameof(DayModel.EventIndicatorTextColor), [nameof(DayModel.EventIndicatorTextColor)] },
        { nameof(DayModel.EventIndicatorSelectedTextColor), [nameof(DayModel.EventIndicatorSelectedTextColor)] },
        { nameof(DayModel.TodayOutlineColor), [nameof(DayModel.TodayOutlineColor), nameof(DayModel.OutlineColor)] },
        { nameof(DayModel.TodayTextColor), [nameof(DayModel.TodayTextColor), nameof(DayModel.TextColor)] },
        { nameof(DayModel.TodayFillColor), [nameof(DayModel.TodayFillColor), nameof(DayModel.BackgroundColor)] },
        { nameof(DayModel.DisabledColor), [nameof(DayModel.DisabledColor)] },
    };

    [Theory]
    [MemberData(nameof(Notifications))]
    public void NewValue_RaisesTheMemberAndTheValuesComputedFromIt(string member, string[] expected)
    {
        var day = new DayModel();
        var property = typeof(DayModel).GetProperty(member)!;
        var value = DifferentValue(property.PropertyType, property.GetValue(day));
        var raised = new List<string>();
        day.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        property.SetValue(day, value);

        raised.Should().Equal(expected);
        property.GetValue(day).Should().Be(value);

        raised.Clear();
        property.SetValue(day, value);

        raised.Should().BeEmpty("setting the same value again changes nothing");
    }

    [Fact]
    public void EverySettableMember_IsListed()
    {
        var settable = typeof(DayModel)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.SetMethod is { IsPublic: true })
            .Select(property => property.Name);

        settable.Should().BeEquivalentTo(Notifications.Select(row => (string)row[0]));
    }

    static object DifferentValue(Type type, object? current) => type switch
    {
        _ when type == typeof(bool) => !(bool)current!,
        _ when type == typeof(DateTime) => new DateTime(2025, 5, 20),
        _ when type == typeof(string) => "20",
        _ when type == typeof(int) => 3,
        _ when type == typeof(double) => 40d,
        _ when type == typeof(float) => 4f,
        _ when type == typeof(Thickness) => new Thickness(1),
        _ when type == typeof(Style) => new Style(typeof(Label)),
        _ when type == typeof(System.Windows.Input.ICommand) => new Command(() => { }),
        _ when type == typeof(IReadOnlyList<object>) => new List<object> { "Meeting" },
        _ when type == typeof(IReadOnlyList<Color>) => new List<Color> { Colors.Red },
        _ when type == typeof(EventIndicatorType) => EventIndicatorType.Background,
        _ when type == typeof(Color) => Color.FromArgb("#123456"),
        _ => throw new NotSupportedException($"Pick a new value for {type}."),
    };
}
