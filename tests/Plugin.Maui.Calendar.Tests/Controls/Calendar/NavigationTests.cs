using System.Windows.Input;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies moving the calendar to another month, week or year: the month and year arrows (the
/// navigation commands), their limits (<c>MinimumDate</c>, <c>MaximumDate</c> and the range of
/// <see cref="DateTime"/>), swipes and the <c>MonthChanged</c> notifications.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class NavigationTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static List<MonthChangedEventArgs> RecordMonthChanges(TestCalendar calendar)
    {
        var changes = new List<MonthChangedEventArgs>();
        calendar.MonthChanged += (_, args) => changes.Add(args);
        return changes;
    }

    static (DateOnly Old, DateOnly New) Months(MonthChangedEventArgs args) => (args.OldMonth, args.NewMonth);

    // ── Month and week arrows ────────────────────────────────────────────────

    [Fact]
    public void MonthArrows_MoveOneMonthAndRaiseMonthChanged()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var changes = RecordMonthChanges(calendar);

        calendar.NextLayoutUnitCommand.Execute(null);

        calendar.ShownDate.Should().Be(new DateTime(2025, 6, 15));

        calendar.PrevLayoutUnitCommand.Execute(null);
        calendar.PrevLayoutUnitCommand.Execute(null);

        calendar.ShownDate.Should().Be(new DateTime(2025, 4, 15));
        changes.Select(Months).Should().Equal(
            (new DateOnly(2025, 5, 15), new DateOnly(2025, 6, 15)),
            (new DateOnly(2025, 6, 15), new DateOnly(2025, 5, 15)),
            (new DateOnly(2025, 5, 15), new DateOnly(2025, 4, 15)));
    }

    [Fact]
    public void MonthArrows_KeepTheDayOrMoveToTheLastDayOfAShorterMonth()
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2025, 1, 31) };

        calendar.NextLayoutUnitCommand.Execute(null);

        calendar.ShownDate.Should().Be(new DateTime(2025, 2, 28));
    }

    [Theory]
    [InlineData(WeekLayout.Week, 7)]
    [InlineData(WeekLayout.TwoWeek, 14)]
    public void WeekArrows_MoveByTheShownWeeks(WeekLayout layout, int days)
    {
        var calendar = new TestCalendar { CalendarLayout = layout, ShownDate = May15 };
        var changes = RecordMonthChanges(calendar);

        calendar.NextLayoutUnitCommand.Execute(null);
        calendar.ShownDate.Should().Be(May15.AddDays(days));

        calendar.PrevLayoutUnitCommand.Execute(null);
        calendar.ShownDate.Should().Be(May15);

        // Moving by a week is reported too (documented), even when the month stays the same.
        changes.Should().HaveCount(2);
    }

    [Fact]
    public void MonthChangedCommand_ReceivesTheEventArgsAndOnlyRunsWhenItCanExecute()
    {
        var executed = new List<MonthChangedEventArgs>();
        var canExecute = true;
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            // CanExecute is asked with the same argument Execute gets, so a typed command can check it.
            MonthChangedCommand = new Command<MonthChangedEventArgs>(executed.Add, args => canExecute && args is not null),
        };

        calendar.NextLayoutUnitCommand.Execute(null);
        canExecute = false;
        calendar.NextLayoutUnitCommand.Execute(null);

        executed.Should().ContainSingle()
            .Which.NewMonth.Should().Be(new DateOnly(2025, 6, 15));
    }

    [Fact]
    public void MonthArrows_WithoutSubscribers_StillMove()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.NextLayoutUnitCommand.Execute(null);

        calendar.ShownDate.Should().Be(new DateTime(2025, 6, 15));
    }

    // ── Limits of the month and week arrows ──────────────────────────────────

    [Fact]
    public void PrevMonthArrow_DisabledWhenThePreviousMonthIsBeforeMinimumDate()
    {
        var calendar = new TestCalendar { ShownDate = May15, MinimumDate = new DateTime(2025, 5, 10) };
        var changes = RecordMonthChanges(calendar);

        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeFalse();
        calendar.PrevLayoutUnitCommand.Execute(null);

        calendar.ShownDate.Should().Be(May15, "April has no day on or after MinimumDate");
        changes.Should().BeEmpty();

        // The last day of April is enough to show April.
        calendar.MinimumDate = new DateTime(2025, 4, 30, 18, 0, 0);
        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void NextMonthArrow_DisabledWhenTheNextMonthIsAfterMaximumDate()
    {
        var calendar = new TestCalendar { ShownDate = May15, MaximumDate = new DateTime(2025, 5, 31) };

        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeFalse();
        calendar.NextLayoutUnitCommand.Execute(null);
        calendar.ShownDate.Should().Be(May15);

        calendar.MaximumDate = new DateTime(2025, 6, 1);
        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void MonthArrows_ShownMonthOutsideTheAllowedDates_CanAlwaysMoveTowardsThem()
    {
        // ShownDate set from code to a month long before MinimumDate.
        var calendar = new TestCalendar { MinimumDate = new DateTime(2025, 5, 1), ShownDate = new DateTime(2024, 1, 15) };

        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeFalse();
        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeTrue("February 2024 is closer to MinimumDate");

        calendar.MinimumDate = DateTime.MinValue;
        calendar.MaximumDate = new DateTime(2023, 12, 31);

        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeFalse();
        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeTrue("December 2023 is closer to MaximumDate");
    }

    [Fact]
    public void WeekArrows_LimitedByTheDaysOfThePreviousOrNextWeek()
    {
        // Thursday May 15 2025; with Sunday first, its week runs from May 11 to May 17.
        var calendar = new TestCalendar
        {
            CalendarLayout = WeekLayout.Week,
            ShownDate = May15,
            MinimumDate = new DateTime(2025, 5, 11),
            MaximumDate = new DateTime(2025, 5, 17),
        };

        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeFalse();
        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeFalse();

        calendar.MinimumDate = new DateTime(2025, 5, 10);
        calendar.MaximumDate = new DateTime(2025, 5, 18);

        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeTrue();
        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void MonthArrows_DisabledAtTheFirstAndLastMonthOfDateTime()
    {
        var calendar = new TestCalendar { ShownDate = DateTime.MinValue };

        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeFalse();
        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeTrue();

        calendar.ShownDate = DateTime.MaxValue.Date;

        calendar.NextLayoutUnitCommand.CanExecute(null).Should().BeFalse();
        calendar.PrevLayoutUnitCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void NavigationCommands_RaiseCanExecuteChangedWhenTheirAnswerMayChange()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var commands = new[]
        {
            calendar.PrevLayoutUnitCommand,
            calendar.NextLayoutUnitCommand,
            calendar.PrevYearCommand,
            calendar.NextYearCommand,
        };
        var raised = commands.ToDictionary(command => command, _ => 0);
        foreach (var command in commands)
        {
            command.CanExecuteChanged += (_, _) => raised[command]++;
        }

        void ExpectRaisedBy(Action change, string because)
        {
            foreach (var command in commands)
            {
                raised[command] = 0;
            }

            change();

            raised.Values.Should().AllSatisfy(count => count.Should().BePositive(), because);
        }

        ExpectRaisedBy(() => calendar.ShownDate = new DateTime(2025, 7, 1), "the shown date changed");
        ExpectRaisedBy(() => calendar.MinimumDate = new DateTime(2025, 6, 1), "the minimum date changed");
        ExpectRaisedBy(() => calendar.MaximumDate = new DateTime(2025, 8, 1), "the maximum date changed");
        ExpectRaisedBy(() => calendar.CalendarLayout = WeekLayout.Week, "the layout changed");
        ExpectRaisedBy(() => calendar.FirstDayOfWeek = DayOfWeek.Monday, "the first day of the week changed");
    }

    // ── Year arrows ──────────────────────────────────────────────────────────

    [Fact]
    public void YearArrows_MoveOneYearAndRaiseMonthChanged()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var changes = RecordMonthChanges(calendar);

        calendar.NextYearCommand.Execute(null);
        calendar.ShownDate.Should().Be(new DateTime(2026, 5, 15));

        calendar.PrevYearCommand.Execute(null);
        calendar.PrevYearCommand.Execute(null);
        calendar.ShownDate.Should().Be(new DateTime(2024, 5, 15));

        changes.Select(Months).Should().Equal(
            (new DateOnly(2025, 5, 15), new DateOnly(2026, 5, 15)),
            (new DateOnly(2026, 5, 15), new DateOnly(2025, 5, 15)),
            (new DateOnly(2025, 5, 15), new DateOnly(2024, 5, 15)));
    }

    [Fact]
    public void YearArrows_FromFebruary29_MoveToFebruary28()
    {
        var calendar = new TestCalendar { ShownDate = new DateTime(2024, 2, 29) };

        calendar.NextYearCommand.Execute(null);

        calendar.ShownDate.Should().Be(new DateTime(2025, 2, 28));
    }

    [Fact]
    public void YearArrows_DisabledWhenTheWholeYearIsOutsideTheAllowedDates()
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            MinimumDate = new DateTime(2025, 1, 1),
            MaximumDate = new DateTime(2025, 12, 31),
        };
        var changes = RecordMonthChanges(calendar);

        calendar.PrevYearCommand.CanExecute(null).Should().BeFalse();
        calendar.NextYearCommand.CanExecute(null).Should().BeFalse();

        calendar.PrevYearCommand.Execute(null);
        calendar.NextYearCommand.Execute(null);

        calendar.ShownDate.Should().Be(May15);
        changes.Should().BeEmpty();
    }

    [Fact]
    public void YearArrows_TargetOutsideTheAllowedDates_MoveToTheNearestAllowedDate()
    {
        var calendar = new TestCalendar
        {
            ShownDate = new DateTime(2025, 3, 10),
            MinimumDate = new DateTime(2024, 6, 15),
            MaximumDate = new DateTime(2026, 2, 20),
        };

        calendar.PrevYearCommand.Execute(null);
        calendar.ShownDate.Should().Be(new DateTime(2024, 6, 15), "March 2024 is before MinimumDate");

        calendar.ShownDate = new DateTime(2025, 3, 10);
        calendar.NextYearCommand.Execute(null);
        calendar.ShownDate.Should().Be(new DateTime(2026, 2, 20), "March 2026 is after MaximumDate");
    }

    [Fact]
    public void YearArrows_DisabledAtTheFirstAndLastYearOfDateTime()
    {
        var calendar = new TestCalendar { ShownDate = DateTime.MinValue };

        calendar.PrevYearCommand.CanExecute(null).Should().BeFalse();
        calendar.NextYearCommand.CanExecute(null).Should().BeTrue();

        calendar.ShownDate = DateTime.MaxValue.Date;

        calendar.NextYearCommand.CanExecute(null).Should().BeFalse();
        calendar.PrevYearCommand.CanExecute(null).Should().BeTrue();
    }

    // ── Changes made from code ───────────────────────────────────────────────

    [Fact]
    public void ShownDateSetFromCode_DoesNotRaiseMonthChangedButExecutesOnShownDateChangedCommand()
    {
        var shownDates = new List<DateTime>();
        var calendar = new TestCalendar { OnShownDateChangedCommand = new Command<DateTime>(shownDates.Add) };
        var changes = RecordMonthChanges(calendar);

        calendar.ShownDate = May15;
        calendar.Month = 7;
        calendar.Year = 2026;

        changes.Should().BeEmpty("MonthChanged reports the user's navigation only");
        shownDates.Should().Equal(May15, new DateTime(2025, 7, 15), new DateTime(2026, 7, 15));
    }

    [Fact]
    public void AutoChangeMonthOnDayTap_TapOnADayOfAnotherMonth_ShowsItsMonthAndRaisesMonthChanged()
    {
        var calendar = new TestCalendar { ShownDate = May15, AutoChangeMonthOnDayTap = true };
        var changes = RecordMonthChanges(calendar);

        calendar.WithTapHandling(() => calendar.CellFor(new DateTime(2025, 6, 2)).Tap());

        calendar.ShownDate.Should().Be(new DateTime(2025, 6, 2));
        calendar.SelectedDate.Should().Be(new DateTime(2025, 6, 2));
        changes.Select(Months).Should().Equal((new DateOnly(2025, 5, 15), new DateOnly(2025, 6, 2)));
    }

    [Fact]
    public void AutoChangeMonthOnDayTap_TapOnADayOfTheShownMonth_KeepsTheMonth()
    {
        var calendar = new TestCalendar { ShownDate = May15, AutoChangeMonthOnDayTap = true };
        var changes = RecordMonthChanges(calendar);

        calendar.WithTapHandling(() => calendar.CellFor(new DateTime(2025, 5, 20)).Tap());

        calendar.ShownDate.Should().Be(May15);
        changes.Should().BeEmpty();
    }

    [Fact]
    public void AutoChangeMonthOnDayTap_Off_TapOnADayOfAnotherMonthKeepsTheMonth()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.WithTapHandling(() => calendar.CellFor(new DateTime(2025, 6, 2)).Tap());

        calendar.ShownDate.Should().Be(May15);
        calendar.SelectedDate.Should().Be(new DateTime(2025, 6, 2));
    }

    // ── Swipes ───────────────────────────────────────────────────────────────

    [Fact]
    public void SwipeLeftAndRight_MoveToTheNextAndPreviousMonthAndRunTheirCommandsAndEvents()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var log = new List<string>();
        calendar.SwipedLeft += (_, _) => log.Add("left event");
        calendar.SwipedRight += (_, _) => log.Add("right event");
        calendar.SwipeLeftCommand = new Command(() => log.Add("left command"));
        calendar.SwipeRightCommand = new Command(() => log.Add("right command"));
        calendar.AttachHandler();

        calendar.Swipe(SwipeDirection.Left);
        calendar.ShownDate.Should().Be(new DateTime(2025, 6, 15));

        calendar.Swipe(SwipeDirection.Right);
        calendar.ShownDate.Should().Be(May15);

        // The calendar's own handler of SwipedLeft/SwipedRight (which runs the command and moves) was
        // subscribed first, when the calendar was created.
        log.Should().Equal("left command", "left event", "right command", "right event");
    }

    [Fact]
    public void SwipeLeftAndRight_WithSwipeToChangeMonthDisabled_OnlyRunTheirCommands()
    {
        var log = new List<string>();
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            SwipeToChangeMonthEnabled = false,
            SwipeLeftCommand = new Command(() => log.Add("left")),
            SwipeRightCommand = new Command(() => log.Add("right")),
        };
        calendar.AttachHandler();

        calendar.Swipe(SwipeDirection.Left);
        calendar.Swipe(SwipeDirection.Right);

        calendar.ShownDate.Should().Be(May15);
        log.Should().Equal("left", "right");
    }

    [Fact]
    public void SwipeLeft_RespectsMaximumDate()
    {
        var calendar = new TestCalendar { ShownDate = May15, MaximumDate = new DateTime(2025, 5, 31) };
        calendar.AttachHandler();

        calendar.Swipe(SwipeDirection.Left);

        calendar.ShownDate.Should().Be(May15);
    }

    [Fact]
    public void SwipeUp_TogglesTheCalendarSectionAndRunsSwipeUpCommand()
    {
        var swipeUps = 0;
        var calendar = new TestCalendar { ShownDate = May15, SwipeUpCommand = new Command(() => swipeUps++) };
        calendar.AttachHandler();

        calendar.Swipe(SwipeDirection.Up);
        calendar.CalendarSectionShown.Should().BeFalse();

        calendar.Swipe(SwipeDirection.Up);
        calendar.CalendarSectionShown.Should().BeTrue();

        swipeUps.Should().Be(2);

        calendar.SwipeUpToHideEnabled = false;
        calendar.Swipe(SwipeDirection.Up);

        calendar.CalendarSectionShown.Should().BeTrue();
        swipeUps.Should().Be(3);
    }

    [Fact]
    public void SwipeDown_RaisesSwipedDownOnly()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var swipedDown = 0;
        calendar.SwipedDown += (_, _) => swipedDown++;
        calendar.AttachHandler();

        calendar.Swipe(SwipeDirection.Down);

        swipedDown.Should().Be(1);
        calendar.ShownDate.Should().Be(May15);
        calendar.CalendarSectionShown.Should().BeTrue();
    }

    [Fact]
    public void Swipes_WithoutCommands_StillMoveAndToggle()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.AttachHandler();

        calendar.Swipe(SwipeDirection.Left);
        calendar.Swipe(SwipeDirection.Left);
        calendar.Swipe(SwipeDirection.Right);
        calendar.Swipe(SwipeDirection.Up);

        calendar.ShownDate.Should().Be(new DateTime(2025, 6, 15));
        calendar.CalendarSectionShown.Should().BeFalse();
    }

    [Fact]
    public void SwipeDown_WithoutSubscribers_DoesNothing()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.AttachHandler();

        // The calendar's own XAML handles left, right and up; down has no handler at all.
        var swipeDown = () => calendar.Swipe(SwipeDirection.Down);

        swipeDown.Should().NotThrow();
    }
}
