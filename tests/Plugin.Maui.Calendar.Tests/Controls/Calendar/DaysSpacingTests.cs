using FluentAssertions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies <see cref="CalendarControl.ColumnSpacing"/> and <see cref="CalendarControl.RowSpacing"/>:
/// the spacing of the days grid, kept across layouts, and the weekend boxes that reach into half
/// the row spacing.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class DaysSpacingTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static Grid Days(CalendarControl calendar) => calendar.Named<Grid>("daysControl");

    static List<Border> WeekendBoxes(CalendarControl calendar) =>
        [.. Days(calendar).Children.OfType<Border>().Where(border => border.InputTransparent)];

    [Fact]
    public void Spacing_DefaultsToNoColumnSpacingAndSixBetweenRows()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.ColumnSpacing.Should().Be(0);
        calendar.RowSpacing.Should().Be(6);
        Days(calendar).ColumnSpacing.Should().Be(0);
        Days(calendar).RowSpacing.Should().Be(6);
    }

    [Fact]
    public void Spacing_ChangedAtRuntime_UpdatesTheGridAndKeepsTheDayCells()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var cells = calendar.DayViews.ToList();

        calendar.ColumnSpacing = 4;
        calendar.RowSpacing = 10;

        Days(calendar).ColumnSpacing.Should().Be(4);
        Days(calendar).RowSpacing.Should().Be(10);
        calendar.DayViews.Should().Equal(cells, "only the spacing changes, not the cells");
    }

    [Fact]
    public void Spacing_IsKeptWhenTheLayoutIsRebuilt()
    {
        var calendar = new TestCalendar { ShownDate = May15, ColumnSpacing = 4, RowSpacing = 10 };

        calendar.CalendarLayout = WeekLayout.TwoWeek;

        Days(calendar).ColumnSpacing.Should().Be(4);
        Days(calendar).RowSpacing.Should().Be(10);
    }

    [Fact]
    public void WeekendBoxes_ReachIntoHalfTheRowSpacing()
    {
        var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };

        WeekendBoxes(calendar).Should().AllSatisfy(box => box.Margin.Should().Be(new Thickness(0, -3, 0, -3)));

        calendar.RowSpacing = 10;

        WeekendBoxes(calendar).Should().HaveCount(12, "two weekend columns in six weeks")
            .And.AllSatisfy(box => box.Margin.Should().Be(new Thickness(0, -5, 0, -5)));
    }
}
