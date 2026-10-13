using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies the day grid drawn with Microsoft.Maui.Graphics (the proof of concept of the migration
/// away from DrawnUI): which grid the calendar shows, where the rows and cells are, taps, and that
/// the grid can be painted.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public sealed class GraphicsDayGridTests : IDisposable
{
	const float width = 350;

	static readonly DateTime May15 = new(2025, 5, 15);

	// The tests of this collection run one at a time, so the process-wide switch only affects these tests.
	public GraphicsDayGridTests() => AppContext.SetSwitch(CalendarControl.GraphicsDayGridSwitch, true);

	public void Dispose() => AppContext.SetSwitch(CalendarControl.GraphicsDayGridSwitch, false);

	static Grid Days(CalendarControl calendar) => calendar.Named<Grid>("daysControl");

	static GraphicsDayGrid Grid(CalendarControl calendar) =>
		calendar.GraphicsDayGrid ?? throw new InvalidOperationException("The calendar does not show the Microsoft.Maui.Graphics grid.");

	[Fact]
	public void Switch_On_ShowsTheGraphicsGrid_AndNoNativeCells()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.GraphicsDayGrid.Should().NotBeNull();
		calendar.DrawnDayGrid.Should().BeNull();
		Days(calendar).Children.Should().ContainSingle().Which.Should().BeSameAs(calendar.GraphicsDayGrid);
		Grid(calendar).GridDrawable.Days.Should().HaveCount(42);
	}

	[Fact]
	public void DayViewTemplate_SwitchesToTheNativeGrid_AndClearingItSwitchesBack()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var grid = calendar.GraphicsDayGrid;

		calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();

		calendar.GraphicsDayGrid.Should().BeNull();
		Days(calendar).Children.OfType<DayView>().Should().HaveCount(42);

		calendar.DayViewTemplate = null;

		calendar.GraphicsDayGrid.Should().BeSameAs(grid);
		grid.GridDrawable.Days.Should().HaveCount(42);
	}

	[Fact]
	public void TheView_IsAsHighAsItsRows()
	{
		var calendar = new TestCalendar { ShownDate = May15, DayViewSize = 40 };
		var grid = Grid(calendar);
		var drawable = grid.GridDrawable;

		// The title row, then 6 weeks of 40 with the row spacing above each.
		var titleRow = drawable.CellBounds(0, width).Top - GraphicsDayGridDrawable.RowSpacing;
		titleRow.Should().BeGreaterThan(0);
		grid.HeightRequest.Should().BeApproximately(titleRow + (6 * (40 + GraphicsDayGridDrawable.RowSpacing)), 0.01);
	}

	[Fact]
	public void Cells_AreOneSeventhWide_InGridOrder()
	{
		var calendar = new TestCalendar { ShownDate = May15, DayViewSize = 40 };
		var drawable = Grid(calendar).GridDrawable;

		drawable.CellBounds(0, width).Left.Should().Be(0);
		drawable.CellBounds(1, width).Left.Should().BeApproximately(width / 7, 0.01f);
		drawable.CellBounds(6, width).Right.Should().BeApproximately(width, 0.01f);
		drawable.CellBounds(7, width).Top.Should().BeApproximately(drawable.CellBounds(0, width).Bottom + GraphicsDayGridDrawable.RowSpacing, 0.01f);
	}

	[Fact]
	public void RightToLeft_MirrorsTheColumns()
	{
		var calendar = new TestCalendar { ShownDate = May15, FlowDirection = FlowDirection.RightToLeft };
		var drawable = Grid(calendar).GridDrawable;

		drawable.IsRightToLeft.Should().BeTrue();
		drawable.CellBounds(0, width).Right.Should().BeApproximately(width, 0.01f);
		drawable.CellBounds(6, width).Left.Should().Be(0);
	}

	[Fact]
	public void AWeekOfHiddenDays_TakesNoHeight()
	{
		var shown = new TestCalendar { ShownDate = May15 };
		var hidden = new TestCalendar { ShownDate = May15, OtherMonthWeekIsVisible = false };

		// May 2025 from Sunday: its sixth week (June 1-7) has no day of May.
		Grid(hidden).GridDrawable.CellBounds(35, width).Height.Should().Be(0);
		Grid(hidden).HeightRequest.Should().BeLessThan(Grid(shown).HeightRequest);
	}

	[Fact]
	public void TheDayUnderAPoint_IsFoundByItsCell()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var drawable = Grid(calendar).GridDrawable;

		for (var index = 0; index < 42; index += 5)
		{
			var center = drawable.CellBounds(index, width).Center;
			drawable.DayAt(new Point(center.X, center.Y), width).Should().BeSameAs(drawable.Days[index]);
		}

		drawable.DayAt(new Point(10, 1), width).Should().BeNull("the title row is no day");
	}

	[Fact]
	public void ATap_SelectsTheDayUnderIt()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var grid = Grid(calendar);
		grid.Frame = new Rect(0, 0, width, grid.HeightRequest);

		var index = grid.GridDrawable.Days.ToList().FindIndex(day => day.Date == new DateTime(2025, 5, 20));
		var center = grid.GridDrawable.CellBounds(index, width).Center;
		calendar.WithTapHandling(() => grid.TapAt(new Point(center.X, center.Y)));

		calendar.SelectedDate.Should().Be(new DateTime(2025, 5, 20));
	}

	[Fact]
	public void TheGrid_CanBePainted()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15,
			SelectedDate = May15,
			WeekendDayBackgroundColor = Colors.LightGray,
			EventIndicatorType = Plugin.Maui.Calendar.Enums.EventIndicatorType.TopDot,
		};
		var grid = Grid(calendar);

		using var canvas = new PictureCanvas(0, 0, width, (float)grid.HeightRequest);
		var paint = () => grid.GridDrawable.Draw(canvas, new RectF(0, 0, width, (float)grid.HeightRequest));

		paint.Should().NotThrow();
	}
}
