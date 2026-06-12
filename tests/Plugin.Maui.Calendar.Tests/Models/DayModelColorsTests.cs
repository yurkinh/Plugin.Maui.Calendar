using FluentAssertions;
using Plugin.Maui.Calendar.Models;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

/// <summary>
/// Verifies that <see cref="DayModel.TextColor"/> and
/// <see cref="DayModel.BackgroundColor"/> honour the disabled-state priority rules:
/// a disabled day always renders with <see cref="DayModel.DisabledColor"/> regardless
/// of weekend or event state.
/// </summary>
public class DayModelColorsTests
{
    static readonly Microsoft.Maui.Graphics.Color DisabledColor =
        Microsoft.Maui.Graphics.Color.FromArgb("#ECECEC");

    static readonly Microsoft.Maui.Graphics.Color WeekendColor =
        Microsoft.Maui.Graphics.Color.FromArgb("#FF0000");

    static readonly Microsoft.Maui.Graphics.Color DeselectedBg =
        Microsoft.Maui.Graphics.Colors.Transparent;

    static DayModel MakeSaturdayModel() => new()
    {
        // Saturday 2025-05-17
        Date = new DateTime(2025, 5, 17),
        IsThisMonth = true,
        OtherMonthIsVisible = true,
        WeekendDayColor = WeekendColor,
        DisabledColor = DisabledColor,
        DeselectedBackgroundColor = DeselectedBg,
    };

    [Fact]
    public void TextColor_WhenWeekendDayAndNotDisabled_ReturnsWeekendDayColor()
    {
        var model = MakeSaturdayModel();
        model.IsDisabled = false;

        model.TextColor.Should().Be(WeekendColor,
            "an enabled weekend day must be coloured with WeekendDayColor");
    }

    [Fact]
    public void TextColor_WhenWeekendDayAndDisabled_ReturnsDisabledColor()
    {
        var model = MakeSaturdayModel();
        model.IsDisabled = true;

        model.TextColor.Should().Be(DisabledColor,
            "IsDisabled takes priority over IsWeekend in the TextColor switch");
    }

    [Fact]
    public void BackgroundColor_WhenDisabled_ReturnsDeselectedBackgroundColor()
    {
        var model = MakeSaturdayModel();
        model.IsDisabled = true;

        model.BackgroundColor.Should().Be(DeselectedBg,
            "a disabled day must not show a selected or event background");
    }
}
