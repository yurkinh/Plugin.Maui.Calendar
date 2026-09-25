using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Models;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies <see cref="CalendarControl.DayViewTemplate"/>: which content each day cell shows,
/// that cells are reused (not rebuilt) when the template or the shown period changes, how a
/// <see cref="DataTemplateSelector"/> is evaluated, and that the shared template is never modified.
/// </summary>
/// <remarks>
/// All dates are fixed. With the default FirstDayOfWeek (Sunday) the month grids are:
/// April 2025 = Mar 30 .. May 10, May 2025 = Apr 27 .. Jun 7, June 2025 = Jun 1 .. Jul 12.
/// </remarks>
[Collection(MauiControlsCollection.Name)]
public class DayViewTemplateTests
{
    static readonly DateTime May15 = new(2025, 5, 15);
    static readonly DateTime MayGridStart = new(2025, 4, 27);
    static readonly DateTime JuneGridStart = new(2025, 6, 1);
    static readonly DateTime AprilGridStart = new(2025, 3, 30);

    static void ShouldShowConsecutiveDays(IDayCells calendar, DateTime firstDate)
    {
        var days = calendar.Days();
        for (var i = 0; i < days.Count; i++)
        {
            days[i].Date.Should().Be(firstDate.AddDays(i), $"cell {i} follows the previous one");
            days[i].Day.Should().Be(firstDate.AddDays(i).Day.ToString());
        }
    }

    // ── template applied to every cell ───────────────────────────────────────

    [Fact]
    public void DayViewTemplate_SetAfterConstruction_EveryCellShowsTemplateForItsOwnDay()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var cellsBefore = calendar.DayViews.ToList();

        calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();
        calendar.EnsureAllContent();

        calendar.DayViews.Should().Equal(cellsBefore, "setting a template must not rebuild the grid");
        calendar.DayViews.Should().HaveCount(42);
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent());
        calendar.DayViews.Should().OnlyContain(dayView => dayView.TemplateContent is Label);
        calendar.DayViews.Select(dayView => dayView.Parent).Distinct().Should().ContainSingle()
            .Which.Should().BeOfType<Grid>("every cell stays in the calendar grid");

        ShouldShowConsecutiveDays(calendar, MayGridStart);
        foreach (var dayView in calendar.DayViews)
        {
            dayView.TemplateContent.BindingContext.Should().BeSameAs(dayView.Day(),
                "the template is bound to the day of its own cell");
            ((Label)dayView.TemplateContent).Text.Should().Be(dayView.Day().Day);
        }

        calendar.Days().Count(day => day.IsThisMonth).Should().Be(31, "May 2025 has 31 days");
    }

    [Fact]
    public void DayViewTemplate_SetInObjectInitializer_CreatesOneViewPerCellAndNoBuiltInCell()
    {
        var spy = new TemplateSpy(CalendarTestExtensions.CreateDayLabel);

        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = spy.Template };

        calendar.DayViews.Should().OnlyContain(dayView => !dayView.IsContentCreated,
            "content is created lazily, right before the first render");
        spy.CreatedCount.Should().Be(0);

        calendar.EnsureAllContent();

        spy.CreatedCount.Should().Be(42, "each cell creates its content once, directly from the template");
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent());
        ShouldShowConsecutiveDays(calendar, MayGridStart);
    }

    [Fact]
    public void DayViewTemplate_OtherMonthDaysHidden_OnlyCurrentMonthCellsAreVisible()
    {
        // ShownDate is set last: it forces the day update that applies OtherMonthDayIsVisible.
        var calendar = new TestCalendar
        {
            OtherMonthDayIsVisible = false,
            DayViewTemplate = CalendarTestExtensions.DayLabelTemplate(),
            ShownDate = May15,
        };
        calendar.EnsureAllContent();

        var visibleDates = calendar.DayViews
            .Where(dayView => dayView.Container().IsVisible)
            .Select(dayView => dayView.Day().Date)
            .ToList();

        visibleDates.Should().HaveCount(31);
        visibleDates.Should().OnlyContain(date => date.Month == 5);
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent(),
            "hidden cells keep their templated content; only the container is hidden");
    }

    [Fact]
    public void DayViewTemplate_OtherMonthDaysHiddenAtRuntime_HidesOtherMonthCells()
    {
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();

        calendar.OtherMonthDayIsVisible = false;

        calendar.DayViews.Count(dayView => dayView.Container().IsVisible).Should().Be(31,
            "the visible cells must follow a new OtherMonthDayIsVisible value");

        calendar.OtherMonthDayIsVisible = true;

        calendar.DayViews.Count(dayView => dayView.Container().IsVisible).Should().Be(42);
    }

    [Fact]
    public void DayViewTemplate_OtherMonthWeeksHiddenAtRuntime_HidesWeekOutsideShownMonth()
    {
        // The last row of the May 2025 grid (Jun 1 .. Jun 7) has no day of May.
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();

        calendar.OtherMonthWeekIsVisible = false;

        calendar.DayViews.Where(dayView => !dayView.IsVisible).Select(dayView => dayView.Day().Date)
            .Should().Equal(Enumerable.Range(1, 7).Select(day => new DateTime(2025, 6, day)),
                "the visible rows must follow a new OtherMonthWeekIsVisible value");
    }

    // ── switching templates ──────────────────────────────────────────────────

    [Fact]
    public void DayViewTemplate_SetBackToNull_RestoresBuiltInCellInSameCells()
    {
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();
        var cellsBefore = calendar.DayViews.ToList();
        var daysBefore = calendar.Days();

        calendar.DayViewTemplate = null;

        calendar.DayViews.Should().Equal(cellsBefore);
        calendar.Days().Should().Equal(daysBefore, "the day models and their state are kept");
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsBuiltInCell());
    }

    [Fact]
    public void DayViewTemplate_DefaultCell_IsBuiltIn()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.EnsureAllContent();

        calendar.DayViewTemplate.Should().BeNull();
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsBuiltInCell());
    }

    [Fact]
    public void DayViewTemplate_SwitchBetweenTemplates_SwapsContentWithoutRebuildingGrid()
    {
        var labels = new TemplateSpy(CalendarTestExtensions.CreateDayLabel);
        var boxes = new TemplateSpy(() => new BoxView());
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = labels.Template };
        calendar.EnsureAllContent();
        var cellsBefore = calendar.DayViews.ToList();
        var daysBefore = calendar.Days();

        calendar.DayViewTemplate = boxes.Template;

        calendar.DayViews.Should().Equal(cellsBefore);
        calendar.Days().Should().Equal(daysBefore);
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent() && dayView.TemplateContent is BoxView);
        boxes.CreatedCount.Should().Be(42);
        ShouldShowConsecutiveDays(calendar, MayGridStart);

        calendar.DayViewTemplate = labels.Template;

        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent() && dayView.TemplateContent is Label);
        labels.CreatedCount.Should().Be(84, "switching back creates new content from the template");
    }

    [Fact]
    public void DayViewTemplate_ChangedBeforeFirstRender_OnlyLatestTemplateIsCreated()
    {
        var first = new TemplateSpy(() => new BoxView());
        var second = new TemplateSpy(CalendarTestExtensions.CreateDayLabel);
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = first.Template };

        calendar.DayViewTemplate = second.Template;
        calendar.EnsureAllContent();

        first.CreatedCount.Should().Be(0, "cells that were never rendered only remember the template");
        second.CreatedCount.Should().Be(42);
    }

    // ── navigation ───────────────────────────────────────────────────────────

    [Fact]
    public void NextAndPrevLayoutUnitCommand_KeepTemplatedContentAndUpdateDays()
    {
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();
        var cellsBefore = calendar.DayViews.ToList();
        var contentBefore = calendar.DayViews.Select(dayView => dayView.TemplateContent).ToList();
        var firstCellChanges = calendar.Days()[0].RecordPropertyChanges();

        calendar.NextLayoutUnitCommand.Execute(null);

        calendar.ShownDate.Should().Be(new DateTime(2025, 6, 15));
        calendar.DayViews.Should().Equal(cellsBefore, "cells are reused on navigation");
        calendar.DayViews.Select(dayView => dayView.TemplateContent).Should().Equal(contentBefore,
            "a plain template's content is kept; only its bindings change");
        ShouldShowConsecutiveDays(calendar, JuneGridStart);
        calendar.Days().Count(day => day.IsThisMonth).Should().Be(30);
        calendar.Days().Where(day => day.IsThisMonth).Should().OnlyContain(day => day.Date.Month == 6);
        foreach (var dayView in calendar.DayViews)
        {
            ((Label)dayView.TemplateContent).Text.Should().Be(dayView.Day().Day);
        }

        // Apr 27 (other month) became Jun 1 (this month).
        firstCellChanges.Should().Contain([
            nameof(ICalendarDay.Date),
            nameof(ICalendarDay.Day),
            nameof(ICalendarDay.IsThisMonth),
        ]);

        calendar.PrevLayoutUnitCommand.Execute(null);
        calendar.PrevLayoutUnitCommand.Execute(null);

        calendar.ShownDate.Should().Be(new DateTime(2025, 4, 15));
        calendar.DayViews.Select(dayView => dayView.TemplateContent).Should().Equal(contentBefore);
        ShouldShowConsecutiveDays(calendar, AprilGridStart);
        calendar.Days().Count(day => day.IsThisMonth).Should().Be(30);
    }

    [Fact]
    public void ShownDate_Changed_TemplatedCellsFollowNewMonth()
    {
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();

        calendar.ShownDate = new DateTime(2025, 6, 3);

        ShouldShowConsecutiveDays(calendar, JuneGridStart);
        calendar.DayViews.Should().OnlyContain(dayView =>
            dayView.ShowsTemplateContent() && ((Label)dayView.TemplateContent).Text == dayView.Day().Day);
    }

    // ── DataTemplateSelector ─────────────────────────────────────────────────

    [Fact]
    public void DataTemplateSelector_ChoosesTemplatePerDate_WithEachCellAsContainer()
    {
        var weekend = CalendarTestExtensions.DayLabelTemplate();
        var weekday = new DataTemplate(() => new BoxView());
        var selector = new RecordingTemplateSelector(day => day.IsWeekend ? weekend : weekday);
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = selector };

        selector.Calls.Should().BeEmpty("no cell content exists before the first render");

        calendar.EnsureAllContent();

        selector.Calls.Should().HaveCount(42);
        selector.Calls.Select(call => call.Container).Should().Equal(calendar.DayViews,
            "the selector receives the cell that hosts the content");
        selector.Calls.Select(call => call.Date).Should().Equal(calendar.Days().Select(day => day.Date));
        foreach (var dayView in calendar.DayViews)
        {
            var isWeekend = dayView.Day().Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            if (isWeekend)
            {
                dayView.TemplateContent.Should().BeOfType<Label>($"{dayView.Day().Date:d} is a weekend day");
            }
            else
            {
                dayView.TemplateContent.Should().BeOfType<BoxView>($"{dayView.Day().Date:d} is a weekday");
            }
        }
    }

    [Fact]
    public void DataTemplateSelector_ReevaluatedForNewDatesAfterNavigation()
    {
        var mayTemplate = CalendarTestExtensions.DayLabelTemplate();
        var otherTemplate = new DataTemplate(() => new BoxView());
        var selector = new RecordingTemplateSelector(day => day.Date.Month == 5 ? mayTemplate : otherTemplate);
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = selector };
        calendar.EnsureAllContent();
        var contentBefore = calendar.DayViews.Select(dayView => dayView.TemplateContent).ToList();
        var showedMay = calendar.Days().Select(day => day.Date.Month == 5).ToList();

        calendar.NextLayoutUnitCommand.Execute(null);

        selector.Calls.Should().HaveCount(84, "every cell moved to another date");
        selector.Calls.Skip(42).Select(call => call.Date).Should().Equal(calendar.Days().Select(day => day.Date),
            "the selector already sees the new date when it is asked again");
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent() && dayView.TemplateContent is BoxView,
            "no day of the June 2025 grid is in May");

        for (var i = 0; i < calendar.DayViews.Count; i++)
        {
            if (showedMay[i])
            {
                calendar.DayViews[i].TemplateContent.Should().NotBeSameAs(contentBefore[i], "the choice changed");
            }
            else
            {
                calendar.DayViews[i].TemplateContent.Should().BeSameAs(contentBefore[i],
                    "the same template was chosen, so the content is kept");
            }
        }

        calendar.PrevLayoutUnitCommand.Execute(null);

        calendar.DayViews.Where(dayView => dayView.Day().Date.Month == 5)
            .Should().HaveCount(31)
            .And.OnlyContain(dayView => dayView.TemplateContent is Label);
    }

    [Fact]
    public void DataTemplateSelector_DayStateChanges_ReevaluatedAndOnlyChangedChoicesSwapContent()
    {
        var eventTemplate = new DataTemplate(() => new BoxView());
        var plainTemplate = CalendarTestExtensions.DayLabelTemplate();
        var selector = new RecordingTemplateSelector(day => day.HasEvents ? eventTemplate : plainTemplate);
        var events = new EventCollection();
        var calendar = new TestCalendar { Events = events, ShownDate = May15, DayViewTemplate = selector };
        calendar.EnsureAllContent();
        var eventCell = calendar.CellFor(new DateTime(2025, 5, 13));
        var contentBefore = calendar.DayViews.Select(dayView => dayView.TemplateContent).ToList();

        events.Add(new DateTime(2025, 5, 13), new List<string> { "event" });

        eventCell.TemplateContent.Should().BeOfType<BoxView>("the selector is asked again once the day has its events");
        ShouldKeepContentExceptIn(eventCell);

        var callsBefore = selector.Calls.Count;
        calendar.SelectedDate = new DateTime(2025, 5, 20);

        selector.Calls.Count.Should().BeGreaterThan(callsBefore, "a selection change updates the days too");
        ShouldKeepContentExceptIn(eventCell);

        events.Remove(new DateTime(2025, 5, 13));

        eventCell.TemplateContent.Should().BeOfType<Label>();

        void ShouldKeepContentExceptIn(DayView changedCell)
        {
            for (var i = 0; i < calendar.DayViews.Count; i++)
            {
                if (!ReferenceEquals(calendar.DayViews[i], changedCell))
                {
                    calendar.DayViews[i].TemplateContent.Should().BeSameAs(contentBefore[i],
                        $"{calendar.Days()[i].Date:d} made the same choice, so its content is kept");
                }
            }
        }
    }

    [Fact]
    public void DataTemplateSelector_AfterNavigation_ChoosesFromCompleteStateOfTheNewDay()
    {
        // Cells are reused: the cell of May 10 (events) shows Jun 14 (none) in the June grid, and the
        // cell of Jun 12 (events) showed May 8 (none). A selector keyed on HasEvents must choose from
        // the new day's state, exactly as when June is opened directly.
        var events = new EventCollection
        {
            [new DateTime(2025, 5, 10)] = new List<string> { "a" },
            [new DateTime(2025, 6, 12)] = new List<string> { "b" },
        };
        var eventTemplate = new DataTemplate(() => new BoxView());
        var plainTemplate = CalendarTestExtensions.DayLabelTemplate();
        var seen = new List<(DateTime Date, bool HasEvents)>();
        var selector = new RecordingTemplateSelector(day =>
        {
            seen.Add((day.Date, day.HasEvents));
            return day.HasEvents ? eventTemplate : plainTemplate;
        });
        var navigated = new TestCalendar { Events = events, ShownDate = May15, DayViewTemplate = selector };
        navigated.EnsureAllContent();

        navigated.NextLayoutUnitCommand.Execute(null);

        var openedDirectly = new TestCalendar { Events = events, ShownDate = new DateTime(2025, 6, 15), DayViewTemplate = selector };
        openedDirectly.EnsureAllContent();

        navigated.CellFor(new DateTime(2025, 6, 12)).TemplateContent.Should().BeOfType<BoxView>();
        navigated.CellFor(new DateTime(2025, 6, 14)).TemplateContent.Should().BeOfType<Label>();
        navigated.DayViews.Select(dayView => dayView.TemplateContent.GetType())
            .Should().Equal(openedDirectly.DayViews.Select(dayView => dayView.TemplateContent.GetType()));
        seen.Should().OnlyContain(call => call.HasEvents == events.ContainsKey(call.Date),
            "the selector never sees a day that still holds the state of the date the cell showed before");
    }

    [Fact]
    public void DataTemplateSelector_TodayMovesAtMidnight_ReevaluatedForTheCellsThatChanged() => CalendarTestExtensions.OnStableDay(today =>
    {
        var todayTemplate = new DataTemplate(() => new BoxView());
        var otherTemplate = CalendarTestExtensions.DayLabelTemplate();
        var selector = new RecordingTemplateSelector(day => day.IsToday ? todayTemplate : otherTemplate);
        var calendar = new TestCalendar { ShownDate = today, DayViewTemplate = selector };
        var todayCell = calendar.CellFor(today);
        var previousTodayCell = calendar.DayViews.First(dayView => dayView.Day().Date != today);

        // The cells are rendered while another visible day is still today, as before midnight.
        todayCell.Day().RefreshIsToday(previousTodayCell.Day().Date);
        previousTodayCell.Day().RefreshIsToday(previousTodayCell.Day().Date);
        calendar.EnsureAllContent();
        previousTodayCell.TemplateContent.Should().BeOfType<BoxView>();

        calendar.RefreshToday();

        todayCell.TemplateContent.Should().BeOfType<BoxView>("the selector is asked again when today moves");
        previousTodayCell.TemplateContent.Should().BeOfType<Label>();
    });

    [Fact]
    public void DataTemplateSelector_NotAskedWhileCellHasNoDay()
    {
        // MemoryToolkit.Maui's Compartmentalize tear-down clears the BindingContext of every view.
        var selector = new RecordingTemplateSelector(day => day.IsWeekend ? CalendarTestExtensions.DayLabelTemplate() : null);
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = selector };
        calendar.EnsureAllContent();
        var cell = calendar.CellFor(new DateTime(2025, 5, 17));
        var content = cell.TemplateContent;
        var callsBefore = selector.Calls.Count;

        var clearBindingContext = () => cell.BindingContext = null;

        clearBindingContext.Should().NotThrow("the selector must not be given a null day");
        selector.Calls.Should().HaveCount(callsBefore);
        cell.TemplateContent.Should().BeSameAs(content, "no new content is built for a cell that is being torn down");
    }

    [Fact]
    public void DataTemplateSelector_ReturningNull_ShowsBuiltInCellForThatDay()
    {
        var special = CalendarTestExtensions.DayLabelTemplate();
        var selector = new RecordingTemplateSelector(day => day.Date.Day == 15 ? special : null);
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = selector };
        calendar.EnsureAllContent();

        ShouldShowTemplateOnlyOnThe15th(calendar);

        calendar.NextLayoutUnitCommand.Execute(null);

        ShouldShowTemplateOnlyOnThe15th(calendar);
        ((Label)calendar.CellFor(new DateTime(2025, 6, 15)).TemplateContent).Text.Should().Be("15");

        static void ShouldShowTemplateOnlyOnThe15th(TestCalendar calendar)
        {
            foreach (var dayView in calendar.DayViews)
            {
                if (dayView.Day().Date.Day == 15)
                {
                    dayView.ShowsTemplateContent().Should().BeTrue($"{dayView.Day().Date:d} gets the template");
                }
                else
                {
                    dayView.ShowsBuiltInCell().Should().BeTrue($"{dayView.Day().Date:d} gets no template");
                }
            }
        }
    }

    [Fact]
    public void DataTemplateSelector_ReplacedByPlainTemplate_UsesPlainTemplateEverywhere()
    {
        var selector = new RecordingTemplateSelector(_ => new DataTemplate(() => new BoxView()));
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = selector };
        calendar.EnsureAllContent();

        calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();
        calendar.NextLayoutUnitCommand.Execute(null);

        selector.Calls.Should().HaveCount(42, "a selector that is no longer set is not asked again");
        calendar.DayViews.Should().OnlyContain(dayView => dayView.TemplateContent is Label);
    }

    // ── the shared template is never modified ────────────────────────────────

    [Fact]
    public void DayViewTemplate_SharedTemplateIsNotModifiedAndDoesNotKeepADay()
    {
        var template = CalendarTestExtensions.DayLabelTemplate();
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = template };
        calendar.EnsureAllContent();
        calendar.NextLayoutUnitCommand.Execute(null);

        template.Values.Should().BeEmpty("no value (such as a BindingContext) may be stamped on a shared template");
        template.Bindings.Should().BeEmpty();
        template.CreateContent().Should().BeOfType<Label>()
            .Which.BindingContext.Should().BeNull("content created elsewhere must not inherit a calendar day");

        calendar.DayViews.Select(dayView => dayView.TemplateContent.BindingContext).Distinct().Should().HaveCount(42,
            "every cell's content is bound to its own day, not to the last one created");
    }

    [Fact]
    public void DataTemplateSelector_ChosenTemplatesAreNotModified()
    {
        var weekend = CalendarTestExtensions.DayLabelTemplate();
        var weekday = new DataTemplate(() => new BoxView());
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            DayViewTemplate = new RecordingTemplateSelector(day => day.IsWeekend ? weekend : weekday),
        };
        calendar.EnsureAllContent();
        calendar.NextLayoutUnitCommand.Execute(null);

        weekend.Values.Should().BeEmpty();
        weekday.Values.Should().BeEmpty();
        ((View)weekend.CreateContent()).BindingContext.Should().BeNull();
    }

    // ── invalid template root ────────────────────────────────────────────────

    // A ViewCell root is the realistic mistake (a template copied from a ListView), and it is not
    // a View; ViewCell is obsolete, which is why it is only referenced here.
#pragma warning disable CS0618
    static DataTemplate ViewCellTemplate() => new(() => new ViewCell { View = new Label() });
#pragma warning restore CS0618

    [Fact]
    public void DayViewTemplate_NonViewRoot_ThrowsInvalidOperationExceptionWhenCellIsCreated()
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            DayViewTemplate = ViewCellTemplate(),
        };

        var createContent = () => calendar.DayViews[0].EnsureContent();

        createContent.Should().Throw<InvalidOperationException>()
            .WithMessage("*DayViewTemplate*must create a View*ViewCell*");
    }

    [Fact]
    public void DayViewTemplate_NonViewRootSetOnRenderedCell_ThrowsAndKeepsCurrentContent()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        calendar.EnsureAllContent();

        var setTemplate = () => calendar.DayViewTemplate = ViewCellTemplate();

        setTemplate.Should().Throw<InvalidOperationException>().WithMessage("*DayViewTemplate*ViewCell*");
        calendar.DayViews[0].ShowsBuiltInCell().Should().BeTrue("the template is validated before the content is replaced");
    }

    [Fact]
    public void DataTemplateSelector_ChoosingNonViewTemplate_Throws()
    {
        var invalid = ViewCellTemplate();
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            DayViewTemplate = new RecordingTemplateSelector(_ => invalid),
        };

        var createContent = () => calendar.DayViews[0].EnsureContent();

        createContent.Should().Throw<InvalidOperationException>().WithMessage("*ViewCell*");
    }

    // ── EventIndicatorType.BackgroundFull ────────────────────────────────────

    [Fact]
    public void BackgroundFullEventColor_PaintsBuiltInCellButNotTemplatedCell()
    {
        var eventDate = new DateTime(2025, 5, 10);
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            EventIndicatorType = EventIndicatorType.BackgroundFull,
            EventIndicatorColor = Colors.Red,
            Events = new EventCollection { [eventDate] = new List<string> { "event" } },
        };
        calendar.EnsureAllContent();
        var eventCell = calendar.CellFor(eventDate);

        eventCell.BackgroundColor.Should().Be(Colors.Red, "the built-in cell paints the event color");

        calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();

        eventCell.BackgroundColor.Should().BeNull("a template draws its own background");

        calendar.EventIndicatorColor = Colors.Blue;

        eventCell.BackgroundColor.Should().BeNull("event color changes must not repaint a templated cell");

        calendar.DayViewTemplate = null;

        eventCell.BackgroundColor.Should().Be(Colors.Blue, "the built-in cell paints the event color again");
    }

    [Fact]
    public void BackgroundFullEventColor_TemplateSetBeforeFirstRender_CellHasNoBackground()
    {
        var eventDate = new DateTime(2025, 5, 10);
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            EventIndicatorType = EventIndicatorType.BackgroundFull,
            EventIndicatorColor = Colors.Red,
            Events = new EventCollection { [eventDate] = new List<string> { "event" } },
            DayViewTemplate = CalendarTestExtensions.DayLabelTemplate(),
        };

        calendar.EnsureAllContent();

        calendar.DayFor(eventDate).HasEvents.Should().BeTrue();
        calendar.CellFor(eventDate).BackgroundColor.Should().BeNull();
    }

    [Fact]
    public void BackgroundFullEventColor_EventIndicatorTypeSwitchedAtRuntime_RepaintsBuiltInCell()
    {
        var eventDate = new DateTime(2025, 5, 10);
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            EventIndicatorColor = Colors.Red,
            Events = new EventCollection { [eventDate] = new List<string> { "event" } },
        };
        calendar.EnsureAllContent();
        var eventCell = calendar.CellFor(eventDate);
        eventCell.BackgroundColor.Should().Be(Colors.Transparent);

        calendar.EventIndicatorType = EventIndicatorType.BackgroundFull;

        eventCell.BackgroundColor.Should().Be(Colors.Red);

        calendar.EventIndicatorType = EventIndicatorType.BottomDot;

        eventCell.BackgroundColor.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void BackgroundFullEventColor_HiddenOtherMonthDay_PaintsNothing()
    {
        // Apr 28 is an other-month day of the May 2025 grid; its row stays visible because it holds May days.
        var hiddenDate = new DateTime(2025, 4, 28);
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            EventIndicatorType = EventIndicatorType.BackgroundFull,
            EventIndicatorColor = Colors.Red,
            Events = new EventCollection { [hiddenDate] = new List<string> { "event" } },
        };
        calendar.EnsureAllContent();
        var cell = calendar.CellFor(hiddenDate);
        cell.BackgroundColor.Should().Be(Colors.Red);

        calendar.OtherMonthDayIsVisible = false;

        cell.IsVisible.Should().BeTrue("the row holds days of May");
        cell.Container().IsVisible.Should().BeFalse();
        cell.BackgroundColor.Should().Be(Colors.Transparent, "a hidden day must not paint its event color");
    }

    // ── EventIndicatorType.TopDot ────────────────────────────────────────────

    [Fact]
    public void BuiltInCell_TopDot_PutsDotRowAboveNumberInEveryCellAndFollowsChanges()
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            EventIndicatorType = EventIndicatorType.TopDot,
            Events = new EventCollection { [new DateTime(2025, 5, 10)] = new List<string> { "event" } },
        };
        calendar.EnsureAllContent();

        static FlexDirection Direction(DayView dayView) => ((FlexLayout)dayView.Container()[1]).Direction;

        calendar.DayViews.Select(Direction).Should().OnlyContain(direction => direction == FlexDirection.ColumnReverse,
            "every cell uses the same direction, so the day numbers stay aligned");

        calendar.NextLayoutUnitCommand.Execute(null);

        calendar.DayViews.Select(Direction).Should().OnlyContain(direction => direction == FlexDirection.ColumnReverse);

        calendar.EventIndicatorType = EventIndicatorType.BottomDot;

        calendar.DayViews.Select(Direction).Should().OnlyContain(direction => direction == FlexDirection.Column);
    }

    // ── taps ─────────────────────────────────────────────────────────────────

    [Fact]
    public void TappingTemplatedCell_SelectsDay()
    {
        var date = new DateTime(2025, 5, 12);
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();
        var cell = calendar.CellFor(date);
        var changes = cell.Day().RecordPropertyChanges();

        calendar.WithTapHandling(cell.Tap);

        cell.Day().IsSelected.Should().BeTrue();
        changes.Should().Contain(nameof(ICalendarDay.IsSelected));
        calendar.SelectedDate.Should().Be(date);
        calendar.Days().Where(day => day.IsSelected).Select(day => day.Date).Should().Equal(date);
        cell.ShowsTemplateContent().Should().BeTrue("tapping does not replace the content");
    }

    [Fact]
    public void TappingDisabledTemplatedCell_DoesNotSelectDay()
    {
        var disabledDate = new DateTime(2025, 5, 20);
        var calendar = new TestCalendar
        {
            DisabledDates = [disabledDate],
            DayViewTemplate = CalendarTestExtensions.DayLabelTemplate(),
            ShownDate = May15,
        };
        calendar.EnsureAllContent();
        var cell = calendar.CellFor(disabledDate);

        calendar.WithTapHandling(cell.Tap);

        cell.Day().IsDisabled.Should().BeTrue();
        cell.Day().IsSelected.Should().BeFalse();
        calendar.SelectedDate.Should().BeNull();
    }

    [Fact]
    public void TappingCell_SelectsDayOnlyInItsOwnCalendar()
    {
        // Every calendar on screen receives the tap message; only the owner of the cell may handle it.
        var date = new DateTime(2025, 5, 12);
        var tapped = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        var other = new TestCalendar { ShownDate = May15 };
        tapped.EnsureAllContent();

        other.WithTapHandling(() => tapped.WithTapHandling(tapped.CellFor(date).Tap));

        tapped.SelectedDate.Should().Be(date);
        other.SelectedDate.Should().BeNull("a tap in one calendar must not select the date in another calendar");
        other.Days().Should().OnlyContain(day => !day.IsSelected);
    }

    [Fact]
    public void DayTappedMessageSentByOtherCode_StillHandledByEveryCalendar()
    {
        var date = new DateTime(2025, 5, 12);
        var first = new TestCalendar { ShownDate = May15 };
        var second = new TestCalendar { ShownDate = May15 };

        first.WithTapHandling(() => second.WithTapHandling(() =>
            WeakReferenceMessenger.Default.Send(new DayTappedMessage(date))));

        first.SelectedDate.Should().Be(date);
        second.SelectedDate.Should().Be(date);
    }

    // ── Week and TwoWeek layouts ─────────────────────────────────────────────

    [Theory]
    [InlineData(WeekLayout.Month, 42, 3, 30)]
    [InlineData(WeekLayout.Week, 7, 4, 27)]
    [InlineData(WeekLayout.TwoWeek, 14, 4, 27)]
    public void DayViewTemplate_InEveryLayout_TemplatesEveryCell(WeekLayout layout, int cellCount, int firstMonth, int firstDay)
    {
        // Wednesday 30 April 2025: the week (Apr 27 - May 3) crosses into May.
        var calendar = new TestCalendar
        {
            CalendarLayout = layout,
            ShownDate = new DateTime(2025, 4, 30),
            DayViewTemplate = CalendarTestExtensions.DayLabelTemplate(),
        };
        calendar.EnsureAllContent();

        calendar.DayViews.Should().HaveCount(cellCount);
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent() && dayView.TemplateContent is Label);
        ShouldShowConsecutiveDays(calendar, new DateTime(2025, firstMonth, firstDay));

        if (layout == WeekLayout.Month)
        {
            calendar.Days().Where(day => day.IsThisMonth).Should().HaveCount(30)
                .And.OnlyContain(day => day.Date.Month == 4);
        }
        else
        {
            calendar.Days().Should().OnlyContain(day => day.IsThisMonth,
                "IsThisMonth is always true in the Week and TwoWeek layouts, even across a month boundary");
        }
    }

    [Theory]
    [InlineData(WeekLayout.Week, 7)]
    [InlineData(WeekLayout.TwoWeek, 14)]
    public void NextLayoutUnitCommand_InWeekLayouts_KeepsTemplatedContent(WeekLayout layout, int cellCount)
    {
        var calendar = new TestCalendar
        {
            CalendarLayout = layout,
            ShownDate = new DateTime(2025, 4, 30),
            DayViewTemplate = CalendarTestExtensions.DayLabelTemplate(),
        };
        calendar.EnsureAllContent();
        var contentBefore = calendar.DayViews.Select(dayView => dayView.TemplateContent).ToList();

        calendar.NextLayoutUnitCommand.Execute(null);

        calendar.DayViews.Select(dayView => dayView.TemplateContent).Should().Equal(contentBefore);
        ShouldShowConsecutiveDays(calendar, new DateTime(2025, 4, 27).AddDays(cellCount));
        calendar.DayViews.Should().OnlyContain(dayView => ((Label)dayView.TemplateContent).Text == dayView.Day().Day);
    }

    [Fact]
    public void CalendarLayoutAndFirstDayOfWeekChanges_RebuildCellsThatKeepTheTemplate()
    {
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();

        calendar.CalendarLayout = WeekLayout.Week;
        calendar.EnsureAllContent();

        calendar.DayViews.Should().HaveCount(7);
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent());
        ShouldShowConsecutiveDays(calendar, new DateTime(2025, 5, 11));

        calendar.CalendarLayout = WeekLayout.TwoWeek;
        calendar.FirstDayOfWeek = DayOfWeek.Monday;
        calendar.EnsureAllContent();

        calendar.DayViews.Should().HaveCount(14);
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent());
        ShouldShowConsecutiveDays(calendar, new DateTime(2025, 5, 12));

        calendar.CalendarLayout = WeekLayout.Month;
        calendar.DayViewTemplate = null;
        calendar.EnsureAllContent();

        calendar.DayViews.Should().HaveCount(42);
        calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsBuiltInCell());
    }

    [Fact]
    public void CalendarLayout_ChangedWhileOnScreen_RendersEachNewCellOnceForItsOwnDate()
    {
        // Thursday 1 May 2025: the May grid and that week both start on Sunday Apr 27, so the first
        // shown date does not change and only a forced day update dates the new cells before they
        // join the (rendered) grid and create their content.
        var selector = new RecordingTemplateSelector(_ => CalendarTestExtensions.DayLabelTemplate());
        var calendar = new TestCalendar { ShownDate = new DateTime(2025, 5, 1), DayViewTemplate = selector };
        var daysGrid = (Grid)calendar.DayViews[0].Parent;
        daysGrid.Handler = new FakeLayoutHandler();

        calendar.CalendarLayout = WeekLayout.Week;

        calendar.DayViews.Should().HaveCount(7)
            .And.OnlyContain(dayView => dayView.ShowsTemplateContent(), "the cells render as they join the grid");
        selector.Calls.Select(call => call.Date).Should().Equal(
            Enumerable.Range(0, 7).Select(offset => new DateTime(2025, 4, 27).AddDays(offset)),
            "each new cell is rendered once, already holding its own date");
    }

    [Fact]
    public void CalendarLayout_Changed_CellsRemovedFromGridNoLongerFollowTheirDay()
    {
        var selector = new RecordingTemplateSelector(_ => CalendarTestExtensions.DayLabelTemplate());
        var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = selector };
        calendar.EnsureAllContent();
        var oldDays = calendar.Days();

        calendar.CalendarLayout = WeekLayout.Week;
        var callsAfterRebuild = selector.Calls.Count;
        oldDays[0].Date = new DateTime(2025, 1, 1);

        selector.Calls.Should().HaveCount(callsAfterRebuild,
            "a cell dropped by a layout rebuild must not keep observing its old day");
    }

    // ── selection calendars ──────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(TestMultiSelectionCalendar))]
    [InlineData(typeof(TestRangeSelectionCalendar))]
    [InlineData(typeof(TestWeekSelectionCalendar))]
    public void SelectionCalendars_TappingTemplatedCell_SelectsAndSurvivesTemplateChanges(Type calendarType)
    {
        var tapped = new DateTime(2025, 5, 12);
        var calendar = (CalendarControl)Activator.CreateInstance(calendarType)!;
        var cells = (IDayCells)calendar;
        calendar.ShownDate = May15;
        calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();
        cells.EnsureAllContent();
        cells.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent());

        calendar.WithTapHandling(cells.CellFor(tapped).Tap);

        // A week selection selects the tapped day's whole week (Sunday May 11 to Saturday May 17).
        var expected = calendar is WeekSelectionCalendar
            ? Enumerable.Range(11, 7).Select(day => new DateTime(2025, 5, day)).ToList()
            : [tapped];
        cells.Days().Where(day => day.IsSelected).Select(day => day.Date).Should().Equal(expected);
        calendar.SelectedDates.Should().BeEquivalentTo(expected);

        calendar.DayViewTemplate = new DataTemplate(() => new BoxView());

        cells.DayViews.Should().OnlyContain(dayView => dayView.TemplateContent is BoxView);
        cells.Days().Where(day => day.IsSelected).Select(day => day.Date).Should().Equal(expected);

        calendar.DayViewTemplate = null;

        cells.DayViews.Should().OnlyContain(dayView => dayView.ShowsBuiltInCell());
        cells.Days().Where(day => day.IsSelected).Select(day => day.Date).Should().Equal(expected,
            "the selection is kept when the template changes");
    }

    [Fact]
    public void RangeSelectionCalendar_TemplatedCellsExposeRangeBoundaries()
    {
        var start = new DateTime(2025, 5, 10);
        var end = new DateTime(2025, 5, 14);
        var calendar = new TestRangeSelectionCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
        calendar.EnsureAllContent();

        calendar.WithTapHandling(calendar.CellFor(start).Tap);

        var startDay = (ICalendarDay)calendar.CellFor(start).TemplateContent.BindingContext;
        startDay.IsRangeStart.Should().BeTrue("a single-day range starts and ends on the same day");
        startDay.IsRangeEnd.Should().BeTrue();

        calendar.WithTapHandling(calendar.CellFor(end).Tap);

        calendar.Days().Where(day => day.IsSelected).Select(day => day.Date)
            .Should().Equal(Enumerable.Range(10, 5).Select(day => new DateTime(2025, 5, day)));
        calendar.Days().Where(day => day.IsRangeStart).Select(day => day.Date).Should().Equal(start);
        calendar.Days().Where(day => day.IsRangeEnd).Select(day => day.Date).Should().Equal(end);
        startDay.IsRangeEnd.Should().BeFalse();

        var middleDay = (ICalendarDay)calendar.CellFor(new DateTime(2025, 5, 12)).TemplateContent.BindingContext;
        middleDay.IsSelected.Should().BeTrue();
        middleDay.IsRangeStart.Should().BeFalse();
        middleDay.IsRangeEnd.Should().BeFalse();
    }
}
