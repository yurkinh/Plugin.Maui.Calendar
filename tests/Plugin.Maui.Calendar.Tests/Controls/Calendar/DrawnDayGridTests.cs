using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies the drawn day grid (the DrawnUI migration): which grid the calendar shows, that the drawn
/// cells show the same day models as the native cells across layout changes, the day-of-week
/// titles, taps, and the switch back to the native grid for a DayViewTemplate.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public sealed class DrawnDayGridTests : IDisposable
{
	static readonly DateTime May15 = new(2025, 5, 15);

	// The tests of this collection run one at a time, so the process-wide switch only affects these tests.
	public DrawnDayGridTests() => AppContext.SetSwitch(CalendarControl.DrawnDayGridSwitch, true);

	public void Dispose() => AppContext.SetSwitch(CalendarControl.DrawnDayGridSwitch, false);

	static Grid Days(CalendarControl calendar) => calendar.Named<Grid>("daysControl");

	// The unit tests run on the net10.0 target, where the drawn grid has no Canvas (see Calendar.DrawnDayGrid).
	static DrawnDayGrid DrawnGrid(CalendarControl calendar) =>
		calendar.DrawnDayGrid ?? throw new InvalidOperationException("The calendar shows the native grid.");

	static DateTime May(int day) => new(2025, 5, day);

	// ── Which grid ───────────────────────────────────────────────────────────

	[Fact]
	public void Switch_On_ShowsTheDrawnGrid_AndNoNativeCells()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.DrawnDayGrid.Should().NotBeNull();
		Days(calendar).Children.OfType<DayView>().Should().BeEmpty();
		Days(calendar).Children.OfType<Label>().Should().BeEmpty();
	}

	[Fact]
	public void Switch_Off_ShowsTheNativeGrid()
	{
		AppContext.SetSwitch(CalendarControl.DrawnDayGridSwitch, false);

		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.DrawnDayGrid.Should().BeNull();
		Days(calendar).Children.OfType<DayView>().Should().HaveCount(42);
	}

	[Fact]
	public void DayViewTemplate_SwitchesToTheNativeGrid_AndClearingItSwitchesBack()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var grid = calendar.DrawnDayGrid;

		calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();

		calendar.DrawnDayGrid.Should().BeNull();
		Days(calendar).Children.OfType<DayView>().Should().HaveCount(42);

		calendar.DayViewTemplate = null;

		calendar.DrawnDayGrid.Should().BeSameAs(grid, "the drawn grid is kept for the life of the calendar");
		DrawnGrid(calendar).Cells.Select(cell => cell.Day).Should().Equal(calendar.Days());
		Days(calendar).Children.OfType<DayView>().Should().BeEmpty();
	}

	[Fact]
	public void DayViewTemplate_AnotherTemplate_KeepsTheNativeGrid()
	{
		var calendar = new TestCalendar { ShownDate = May15, DayViewTemplate = CalendarTestExtensions.DayLabelTemplate() };
		var cells = calendar.DayViews.ToList();

		calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();

		calendar.DayViews.Should().Equal(cells, "only the content of the native cells changes");
	}

	// ── Days ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Cells_ShowTheDayModelsOfTheCalendar_InGridOrder()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		DrawnGrid(calendar).Cells.Select(cell => cell.Day).Should().Equal(calendar.Days());
		calendar.Days().Should().HaveCount(42);
	}

	[Fact]
	public void Cells_ShowTheDayNumbers()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		DrawnGrid(calendar).Cells.Select(cell => cell.Label.Text)
			.Should().Equal(calendar.Days().Select(day => day.Day));
	}

	[Fact]
	public void MonthChange_KeepsTheCells_AndShowsTheNewDays()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var cells = DrawnGrid(calendar).Cells.ToList();

		calendar.NextLayoutUnitCommand.Execute(null);

		DrawnGrid(calendar).Cells.Should().Equal(cells);
		DrawnGrid(calendar).Cells.Select(cell => cell.Label.Text)
			.Should().Equal(calendar.Days().Select(day => day.Day));
		calendar.VisibleStartDate.Should().Be(new DateTime(2025, 6, 1));
	}

	[Theory]
	[InlineData(WeekLayout.Week, 7)]
	[InlineData(WeekLayout.TwoWeek, 14)]
	[InlineData(WeekLayout.Month, 42)]
	public void CalendarLayout_ShowsTheDaysOfTheLayout(WeekLayout layout, int days)
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.CalendarLayout = layout;

		DrawnGrid(calendar).Cells.Should().HaveCount(days);
		DrawnGrid(calendar).Cells.Select(cell => cell.Day).Should().Equal(calendar.Days());
	}

	[Fact]
	public void DayState_ChangedByTheCalendar_IsShownByTheCell()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.SelectedDate = May(20);

		var cell = DrawnGrid(calendar).Cells.Single(cell => cell.Day.Date == May(20));
		cell.BackgroundShape.BackgroundColor.Should().Be(cell.Day.SelectedBackgroundColor);
		cell.Label.TextColor.Should().Be(cell.Day.TextColor);
	}

	[Fact]
	public void Tap_SelectsTheDay()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.WithTapHandling(() => DrawnGrid(calendar).Cells.Single(cell => cell.Day.Date == May(20)).Tap());

		calendar.SelectedDate.Should().Be(May(20));
	}

	[Fact]
	public void Tap_OnADisabledDay_DoesNothing()
	{
		var calendar = new TestCalendar { ShownDate = May15, DisabledDates = [May(20)] };

		calendar.WithTapHandling(() => DrawnGrid(calendar).Cells.Single(cell => cell.Day.Date == May(20)).Tap());

		calendar.SelectedDate.Should().BeNull();
	}

	// ── Titles ───────────────────────────────────────────────────────────────

	[Fact]
	public void Titles_FollowFirstDayOfWeekAndTheTitleOptions()
	{
		var calendar = new TestCalendar { ShownDate = May15, FirstDayOfWeek = DayOfWeek.Monday };

		calendar.DaysTitleLabelFirstUpperRestLower = true;

		DrawnGrid(calendar).TitleLabels.Select(label => label.Text)
			.Should().Equal("Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun");
	}

	[Fact]
	public void Titles_UseDaysTitleLabelStyle_AndWeekendTitleStyleOnWeekendDays()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.DaysTitleLabelStyle = LabelStyle(Colors.Green);
		calendar.WeekendTitleStyle = LabelStyle(Colors.Red);

		// Sunday first: the first and the last column are weekend days.
		DrawnGrid(calendar).TitleLabels.Select(label => label.TextColor)
			.Should().Equal(Colors.Red, Colors.Green, Colors.Green, Colors.Green, Colors.Green, Colors.Green, Colors.Red);
	}

	static Style LabelStyle(Color textColor)
	{
		var style = new Style(typeof(Label));
		style.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = textColor });
		return style;
	}
}
