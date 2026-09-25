using FluentAssertions;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

/// <summary>
/// Verifies <see cref="DayModel"/> as the <see cref="ICalendarDay"/> a <c>DayViewTemplate</c> binds
/// to: the values of the public members and the PropertyChanged notifications they raise.
/// All dates are fixed and in the past, so no test depends on the current date.
/// </summary>
public class DayModelCalendarDayTests
{
    // Thursday; a past date, so it is never today when the tests run.
    static readonly DateTime Thursday = new(2025, 5, 15);

    // ── public contract ──────────────────────────────────────────────────────

    [Fact]
    public void ICalendarDay_ExposesExpectedReadOnlyMembers()
    {
        var members = typeof(ICalendarDay).GetProperties();

        members.Select(member => member.Name).Should().BeEquivalentTo([
            nameof(ICalendarDay.Date),
            nameof(ICalendarDay.Day),
            nameof(ICalendarDay.IsSelected),
            nameof(ICalendarDay.IsToday),
            nameof(ICalendarDay.IsWeekend),
            nameof(ICalendarDay.IsThisMonth),
            nameof(ICalendarDay.IsDisabled),
            nameof(ICalendarDay.HasEvents),
            nameof(ICalendarDay.EventCount),
            nameof(ICalendarDay.Events),
            nameof(ICalendarDay.EventColors),
            nameof(ICalendarDay.IsRangeStart),
            nameof(ICalendarDay.IsRangeEnd),
        ]);
        members.Should().OnlyContain(member => member.CanRead && !member.CanWrite,
            "templates read the day's state; only the calendar changes it");
    }

    [Fact]
    public void ICalendarDay_ReturnsTheModelValues()
    {
        IReadOnlyList<Color> eventColors = [Colors.Red];
        IReadOnlyList<object> events = ["standup"];
        var model = new DayModel
        {
            Date = Thursday,
            Day = "15",
            IsSelected = true,
            IsThisMonth = true,
            IsDisabled = true,
            HasEvents = true,
            EventCount = 4,
            Events = events,
            EventColors = eventColors,
            IsRangeStart = true,
            IsRangeEnd = true,
        };

        ICalendarDay day = model;

        day.Date.Should().Be(Thursday);
        day.Day.Should().Be("15");
        day.IsSelected.Should().BeTrue();
        day.IsThisMonth.Should().BeTrue();
        day.IsDisabled.Should().BeTrue();
        day.HasEvents.Should().BeTrue();
        day.EventCount.Should().Be(4);
        day.Events.Should().BeSameAs(events);
        day.EventColors.Should().BeSameAs(eventColors);
        day.IsRangeStart.Should().BeTrue();
        day.IsRangeEnd.Should().BeTrue();
        day.IsToday.Should().BeFalse();
        day.IsWeekend.Should().BeFalse();
    }

    [Theory]
    [InlineData(nameof(ICalendarDay.Date))]
    [InlineData(nameof(ICalendarDay.Day))]
    [InlineData(nameof(ICalendarDay.IsSelected))]
    [InlineData(nameof(ICalendarDay.IsThisMonth))]
    [InlineData(nameof(ICalendarDay.IsDisabled))]
    [InlineData(nameof(ICalendarDay.HasEvents))]
    [InlineData(nameof(ICalendarDay.EventCount))]
    [InlineData(nameof(ICalendarDay.Events))]
    [InlineData(nameof(ICalendarDay.EventColors))]
    [InlineData(nameof(ICalendarDay.IsRangeStart))]
    [InlineData(nameof(ICalendarDay.IsRangeEnd))]
    public void SettingMember_RaisesPropertyChangedForIt(string member)
    {
        var model = new DayModel { Date = Thursday };
        var changes = model.RecordPropertyChanges();

        switch (member)
        {
            case nameof(ICalendarDay.Date): model.Date = Thursday.AddDays(1); break;
            case nameof(ICalendarDay.Day): model.Day = "16"; break;
            case nameof(ICalendarDay.IsSelected): model.IsSelected = true; break;
            case nameof(ICalendarDay.IsThisMonth): model.IsThisMonth = true; break;
            case nameof(ICalendarDay.IsDisabled): model.IsDisabled = true; break;
            case nameof(ICalendarDay.HasEvents): model.HasEvents = true; break;
            case nameof(ICalendarDay.EventCount): model.EventCount = 2; break;
            case nameof(ICalendarDay.Events): model.Events = ["standup"]; break;
            case nameof(ICalendarDay.EventColors): model.EventColors = [Colors.Red]; break;
            case nameof(ICalendarDay.IsRangeStart): model.IsRangeStart = true; break;
            case nameof(ICalendarDay.IsRangeEnd): model.IsRangeEnd = true; break;
            default: throw new ArgumentOutOfRangeException(nameof(member), member, null);
        }

        changes.Should().Contain(member);
    }

    [Fact]
    public void SettingMemberToSameValue_RaisesNothing()
    {
        var model = new DayModel { Date = Thursday, IsSelected = true, EventCount = 2 };
        var changes = model.RecordPropertyChanges();

        model.Date = Thursday;
        model.IsSelected = true;
        model.EventCount = 2;

        changes.Should().BeEmpty();
    }

    // ── IsWeekend ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(12, false)] // Monday
    [InlineData(13, false)]
    [InlineData(14, false)]
    [InlineData(15, false)]
    [InlineData(16, false)] // Friday
    [InlineData(17, true)]  // Saturday
    [InlineData(18, true)]  // Sunday
    public void IsWeekend_TrueOnlyOnSaturdayAndSunday(int dayOfMay, bool expected)
    {
        ICalendarDay day = new DayModel { Date = new DateTime(2025, 5, dayOfMay) };

        day.IsWeekend.Should().Be(expected);
    }

    [Fact]
    public void IsWeekend_DoesNotDependOnWeekendDayColor()
    {
        var saturday = new DayModel { Date = new DateTime(2025, 5, 17), WeekendDayColor = Colors.Transparent };

        saturday.IsWeekend.Should().BeTrue("a template can style weekends without a WeekendDayColor");
        saturday.IsWeekendColored.Should().BeFalse("the built-in weekend text color needs a visible WeekendDayColor");

        saturday.WeekendDayColor = Colors.Red;

        saturday.IsWeekend.Should().BeTrue();
        saturday.IsWeekendColored.Should().BeTrue();
    }

    [Fact]
    public void TextColor_WeekendWithoutWeekendDayColor_UsesDeselectedTextColor()
    {
        var saturday = new DayModel
        {
            Date = new DateTime(2025, 5, 17),
            IsThisMonth = true,
            OtherMonthIsVisible = true,
            DeselectedTextColor = Colors.Black,
        };

        saturday.TextColor.Should().Be(Colors.Black);
    }

    // ── Date changes (cell reuse) ────────────────────────────────────────────

    [Fact]
    public void DateChange_RaisesEveryDateDependentMember()
    {
        var model = new DayModel { Date = new DateTime(2025, 5, 16) };
        var changes = model.RecordPropertyChanges();

        model.Date = new DateTime(2025, 5, 17);

        changes.Should().Contain([
            nameof(ICalendarDay.Date),
            nameof(ICalendarDay.IsWeekend),
            nameof(ICalendarDay.IsToday),
            nameof(DayModel.TextColor),
            nameof(DayModel.BackgroundColor),
            nameof(DayModel.OutlineColor),
        ]);
    }

    // ── IsToday ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsToday_PastDate_IsFalse()
    {
        ICalendarDay day = new DayModel { Date = Thursday };

        day.IsToday.Should().BeFalse();
    }

    [Fact]
    public void RefreshIsToday_WhenValueChanges_RaisesIsTodayAndDependentColors()
    {
        var model = new DayModel { Date = Thursday };
        var changes = model.RecordPropertyChanges();

        model.RefreshIsToday(Thursday);

        model.IsToday.Should().BeTrue();
        changes.Should().BeEquivalentTo([
            nameof(ICalendarDay.IsToday),
            nameof(DayModel.BackgroundColor),
            nameof(DayModel.OutlineColor),
            nameof(DayModel.TextColor),
        ]);

        changes.Clear();
        model.RefreshIsToday(Thursday.AddDays(1));

        model.IsToday.Should().BeFalse();
        changes.Should().Contain(nameof(ICalendarDay.IsToday));
    }

    [Fact]
    public void RefreshIsToday_WhenValueDoesNotChange_RaisesNothing()
    {
        var model = new DayModel { Date = Thursday };
        model.RefreshIsToday(Thursday);
        var changes = model.RecordPropertyChanges();

        model.RefreshIsToday(Thursday.AddHours(13));

        model.IsToday.Should().BeTrue("only the date part of today is compared");
        changes.Should().BeEmpty();
    }

    [Fact]
    public void OutlineColor_FollowsRefreshedIsToday()
    {
        var model = new DayModel { Date = Thursday, TodayOutlineColor = Colors.Red };
        model.OutlineColor.Should().Be(Colors.Transparent);

        model.RefreshIsToday(Thursday);

        model.OutlineColor.Should().Be(Colors.Red);

        model.IsSelected = true;

        model.OutlineColor.Should().Be(Colors.Transparent, "a selected day shows no today outline");
    }

    // ── built-in cell: event indicator layout and BackgroundFull ─────────────

    [Theory]
    [InlineData(EventIndicatorType.BottomDot, false, FlexDirection.Column)]
    [InlineData(EventIndicatorType.BottomDot, true, FlexDirection.Column)]
    [InlineData(EventIndicatorType.TopDot, false, FlexDirection.ColumnReverse)]
    [InlineData(EventIndicatorType.TopDot, true, FlexDirection.ColumnReverse)]
    public void EventLayoutDirection_TopDotPutsDotRowAboveNumberInEveryCell(EventIndicatorType type, bool hasEvents, FlexDirection expected)
    {
        // Every cell uses the same direction, so day numbers stay aligned whether or not a day has events.
        var model = new DayModel { EventIndicatorType = type, HasEvents = hasEvents };

        model.EventLayoutDirection.Should().Be(expected);
    }

    [Fact]
    public void EventIndicatorTypeChange_RaisesEventLayoutDirectionAndBackgroundFullEventColor()
    {
        var model = new DayModel { Date = Thursday, IsThisMonth = true, HasEvents = true, EventIndicatorColor = Colors.Red };
        var changes = model.RecordPropertyChanges();

        model.EventIndicatorType = EventIndicatorType.TopDot;

        changes.Should().Contain(nameof(DayModel.EventLayoutDirection));

        changes.Clear();
        model.EventIndicatorType = EventIndicatorType.BackgroundFull;

        model.BackgroundFullEventColor.Should().Be(Colors.Red);
        changes.Should().Contain([nameof(DayModel.BackgroundFullEventColor), nameof(DayModel.EventLayoutDirection)]);

        changes.Clear();
        model.EventIndicatorType = EventIndicatorType.BottomDot;

        model.BackgroundFullEventColor.Should().Be(Colors.Transparent);
        changes.Should().Contain(nameof(DayModel.BackgroundFullEventColor));
    }

    [Fact]
    public void BackgroundFullEventColor_HiddenOtherMonthDay_IsTransparent()
    {
        var model = new DayModel
        {
            Date = Thursday,
            IsThisMonth = true,
            OtherMonthIsVisible = false,
            HasEvents = true,
            EventIndicatorType = EventIndicatorType.BackgroundFull,
            EventIndicatorColor = Colors.Red,
        };
        model.BackgroundFullEventColor.Should().Be(Colors.Red);
        var changes = model.RecordPropertyChanges();

        model.IsThisMonth = false;

        model.IsVisible.Should().BeFalse();
        model.BackgroundFullEventColor.Should().Be(Colors.Transparent, "a hidden day paints nothing, not even its event color");
        changes.Should().Contain(nameof(DayModel.BackgroundFullEventColor));

        changes.Clear();
        model.OtherMonthIsVisible = true;

        model.BackgroundFullEventColor.Should().Be(Colors.Red);
        changes.Should().Contain(nameof(DayModel.BackgroundFullEventColor));
    }
}
