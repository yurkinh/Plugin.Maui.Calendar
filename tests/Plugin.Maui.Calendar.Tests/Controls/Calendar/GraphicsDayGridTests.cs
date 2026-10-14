using System.Reflection;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies the day grid drawn with Microsoft.Maui.Graphics: which grid the calendar shows, that it shows
/// the same day models as the native cells across layout changes, what it draws for a day, a title and a
/// weekend band (the drawable's description of them, which it paints), where the rows and cells are, taps,
/// swipes, right to left, and what a screen reader finds.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public sealed class GraphicsDayGridTests : IDisposable
{
	const float width = 350;

	static readonly DateTime May15 = new(2025, 5, 15);

	// The tests of this collection run one at a time, so the process-wide switch only affects these tests.
	public GraphicsDayGridTests() => AppContext.SetSwitch(CalendarControl.GraphicsDayGridSwitch, true);

	public void Dispose() => AppContext.SetSwitch(CalendarControl.GraphicsDayGridSwitch, false);

	static DateTime May(int day) => new(2025, 5, day);

	static Grid Days(CalendarControl calendar) => calendar.Named<Grid>("daysControl");

	static GraphicsDayGrid Grid(CalendarControl calendar) =>
		calendar.GraphicsDayGrid ?? throw new InvalidOperationException("The calendar does not show the Microsoft.Maui.Graphics grid.");

	static GraphicsDayGridDrawable Drawable(CalendarControl calendar) => Grid(calendar).GridDrawable;

	static int IndexOf(CalendarControl calendar, DateTime date) =>
		Drawable(calendar).Days.ToList().FindIndex(day => day.Date == date);

	// What the grid draws for the day, in a grid `width` wide.
	static GraphicsDayGridDrawable.DayLook Look(CalendarControl calendar, DateTime date) =>
		Drawable(calendar).DayLookOf(IndexOf(calendar, date), width);

	static IEnumerable<GraphicsDayGridDrawable.TitleLook> Titles(CalendarControl calendar) =>
		Enumerable.Range(0, 7).Select(column => Drawable(calendar).TitleLookOf(column, width));

	// The column a rectangle of the grid is in.
	static int ColumnOf(RectF bounds) => (int)Math.Round(bounds.Left / (width / 7));

	// ── Which grid ───────────────────────────────────────────────────────────

	[Fact]
	public void Switch_On_ShowsTheGraphicsGrid_AndNoNativeCells()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.GraphicsDayGrid.Should().NotBeNull();
		calendar.DrawnDayGrid.Should().BeNull();
		Days(calendar).Children.Should().ContainSingle().Which.Should().BeSameAs(calendar.GraphicsDayGrid);
		Drawable(calendar).Days.Should().HaveCount(42);
	}

	[Fact]
	public void Switch_Off_ShowsTheNativeGrid()
	{
		AppContext.SetSwitch(CalendarControl.GraphicsDayGridSwitch, false);

		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.GraphicsDayGrid.Should().BeNull();
		Days(calendar).Children.OfType<DayView>().Should().HaveCount(42);
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

		calendar.GraphicsDayGrid.Should().BeSameAs(grid, "the drawn grid is kept for the life of the calendar");
		Drawable(calendar).Days.Should().Equal(calendar.Days());
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

	[Fact]
	public void DayViewTemplateSelector_UsesTheNativeGrid_AndItsChoices()
	{
		var label = CalendarTestExtensions.DayLabelTemplate();
		var selector = new RecordingTemplateSelector(day => day.IsSelected ? label : null);

		var calendar = new TestCalendar { ShownDate = May15, SelectedDate = May(14), DayViewTemplate = selector };
		calendar.EnsureAllContent();

		calendar.GraphicsDayGrid.Should().BeNull();
		calendar.CellFor(May(14)).ShowsTemplateContent().Should().BeTrue("the selector chose the template for the selected day");
		calendar.CellFor(May(13)).ShowsBuiltInCell().Should().BeTrue("a selector returning null gives the built-in cell");
	}

	[Fact]
	public void SwitchingGrids_KeepsTheCalendarState()
	{
		var calendar = new TestRangeSelectionCalendar
		{
			ShownDate = May15, SelectedStartDate = May(12), SelectedEndDate = May(16),
			SelectedDatesRangeBackgroundColor = Colors.LightBlue,
			Events = new Plugin.Maui.Calendar.Models.EventCollection { [May(20)] = new List<string> { "Event" } },
		};
		calendar.NextLayoutUnitCommand.Execute(null);
		calendar.PrevLayoutUnitCommand.Execute(null);

		calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();
		calendar.DayViewTemplate = null;

		Drawable(calendar).Days.Should().Equal(calendar.Days(), "the drawn grid shows the new days");
		Look(calendar, May(14)).Day.IsSelected.Should().BeTrue();
		Look(calendar, May(14)).Background.Should().Be(Colors.LightBlue, "the range colors are applied to the new days");
		Look(calendar, May(20)).Day.HasEvents.Should().BeTrue();
		calendar.ShownDate.Month.Should().Be(5);
	}

	[Fact]
	public void SwitchingToTheNativeGrid_ReleasesTheDrawnDays()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var grid = Grid(calendar);

		calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();

		grid.GridDrawable.Days.Should().BeEmpty("the drawn grid no longer follows days the calendar replaced");
	}

	[Fact]
	public void DayViewTemplateSetAfterTheConstructor_AsXamlDoes_EndsInTheNativeGrid()
	{
		// XAML assigns the template after the constructor, which already rendered the drawn grid.
		var calendar = new TestCalendar();
		calendar.ShownDate = May15;
		calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();
		calendar.EnsureAllContent();

		calendar.IsDrawnDayGridCreated.Should().BeFalse("the template came before the calendar was shown");
		calendar.GraphicsDayGrid.Should().BeNull();
		calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent());
		Days(calendar).Children.OfType<DayView>().Should().HaveCount(42);
	}

	[Fact]
	public void DrawnGrid_IsCreatedWhenTheCalendarGetsAHandler()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };
		calendar.IsDrawnDayGridCreated.Should().BeFalse("nothing is drawn before the calendar is shown");

		calendar.SimulateHandlerAttached();

		calendar.IsDrawnDayGridCreated.Should().BeTrue();
		Drawable(calendar).Days.Should().Equal(calendar.Days());
		Titles(calendar).ElementAt(1).Text.Should().Be("MON");
		Drawable(calendar).WeekendBands(width).Should().HaveCount(12);
	}

	// ── Days ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Days_AreTheDayModelsOfTheCalendar_InGridOrder()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		Drawable(calendar).Days.Should().Equal(calendar.Days());
		calendar.Days().Should().HaveCount(42);
	}

	[Fact]
	public void Days_ShowTheDayNumbers()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		Enumerable.Range(0, 42).Select(index => Drawable(calendar).DayLookOf(index, width).Text)
			.Should().Equal(calendar.Days().Select(day => day.Day));
	}

	[Fact]
	public void MonthChange_KeepsTheGrid_AndShowsTheNewDays()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var grid = Grid(calendar);

		calendar.NextLayoutUnitCommand.Execute(null);

		Grid(calendar).Should().BeSameAs(grid);
		Drawable(calendar).Days.Should().Equal(calendar.Days());
		Enumerable.Range(0, 42).Select(index => Drawable(calendar).DayLookOf(index, width).Text)
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

		Drawable(calendar).Days.Should().HaveCount(days).And.Equal(calendar.Days());
		Drawable(calendar).Weeks.Should().Be(days / 7);
	}

	[Fact]
	public void DayState_ChangedByTheCalendar_IsDrawn()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.SelectedDate = May(20);

		var look = Look(calendar, May(20));
		look.Background.Should().Be(look.Day.SelectedBackgroundColor);
		look.TextColor.Should().Be(look.Day.TextColor);
	}

	[Fact]
	public void TheView_IsAsHighAsItsRows()
	{
		var calendar = new TestCalendar { ShownDate = May15, DayViewSize = 40 };
		var grid = Grid(calendar);

		// The title row, then 6 weeks of 40 with the row spacing above each.
		var titleRow = grid.GridDrawable.CellBounds(0, width).Top - GraphicsDayGridDrawable.RowSpacing;
		titleRow.Should().BeGreaterThan(0);
		grid.HeightRequest.Should().BeApproximately(titleRow + (6 * (40 + GraphicsDayGridDrawable.RowSpacing)), 0.01);
	}

	[Fact]
	public void Cells_AreOneSeventhWide_InGridOrder()
	{
		var calendar = new TestCalendar { ShownDate = May15, DayViewSize = 40 };
		var drawable = Drawable(calendar);

		drawable.CellBounds(0, width).Left.Should().Be(0);
		drawable.CellBounds(1, width).Left.Should().BeApproximately(width / 7, 0.01f);
		drawable.CellBounds(6, width).Right.Should().BeApproximately(width, 0.01f);
		drawable.CellBounds(7, width).Top.Should().BeApproximately(drawable.CellBounds(0, width).Bottom + GraphicsDayGridDrawable.RowSpacing, 0.01f);
	}

	// ── Taps ─────────────────────────────────────────────────────────────────

	[Fact]
	public void TheDayUnderAPoint_IsFoundByItsCell()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var drawable = Drawable(calendar);

		for (var index = 0; index < 42; index += 5)
		{
			var center = drawable.CellBounds(index, width).Center;
			drawable.DayAt(new Point(center.X, center.Y), width).Should().BeSameAs(drawable.Days[index]);
		}

		drawable.DayAt(new Point(10, 1), width).Should().BeNull("the title row is no day");
	}

	// Taps the center of the cell of date, as a touch does.
	static void TapDay(CalendarControl calendar, DateTime date)
	{
		var grid = Grid(calendar);
		grid.Frame = new Rect(0, 0, width, grid.HeightRequest);
		var center = grid.GridDrawable.CellBounds(IndexOf(calendar, date), width).Center;
		calendar.WithTapHandling(() => grid.TapAt(new Point(center.X, center.Y)));
	}

	[Fact]
	public void Tap_SelectsTheDayUnderIt()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		TapDay(calendar, May(20));

		calendar.SelectedDate.Should().Be(May(20));
	}

	[Fact]
	public void Tap_OnADisabledDay_DoesNothing()
	{
		var calendar = new TestCalendar { ShownDate = May15, DisabledDates = [May(20)] };

		TapDay(calendar, May(20));

		calendar.SelectedDate.Should().BeNull();
	}

	// ── The built-in cell ────────────────────────────────────────────────────

	[Fact]
	public void Cell_FillsItsColumn_WithADayViewSizeByDayViewHeightContainer()
	{
		var calendar = new TestCalendar { ShownDate = May15, DayViewSize = 32, DayViewHeight = 56 };

		var look = Look(calendar, May(14));

		look.Cell.Width.Should().BeApproximately(width / 7, 0.01f);
		look.Cell.Height.Should().Be(56);
		look.Container.Width.Should().Be(32);
		look.Container.Height.Should().Be(56);
		look.Container.Center.X.Should().BeApproximately(look.Cell.Center.X, 0.01f);
	}

	[Fact]
	public void Background_FillsTheContainerUpToDayViewSize_InsideDayViewBorderMargin()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, DayViewSize = 40, DayViewHeight = 60, DayViewBorderMargin = new Thickness(2, 3), DayViewCornerRadius = 6,
		};

		var look = Look(calendar, May(14));

		// Width: 40 less the horizontal margin; height: the native Border's maximum, DayViewSize, in
		// the middle of the container less its vertical margin.
		look.Border.Width.Should().Be(36);
		look.Border.Height.Should().Be(40);
		look.Border.Left.Should().BeApproximately(look.Container.Left + 2, 0.01f);
		look.Border.Center.Y.Should().BeApproximately(look.Container.Center.Y, 0.01f);
		look.CornerRadius.Should().Be(6);
	}

	[Fact]
	public void Outline_ShowsTheTodayOutline_OnlyOnTodayWhileNotSelected()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, TodayOutlineColor = Colors.Green, TimeProvider = new FixedTimeProvider(May(14)),
		};

		Look(calendar, May(14)).Outline.Should().Be(Colors.Green);
		Look(calendar, May(15)).Outline.Should().Be(Colors.Transparent);

		calendar.SelectedDate = May(14);

		Look(calendar, May(14)).Outline.Should().Be(Colors.Transparent);
	}

	[Fact]
	public void BackgroundFull_PaintsTheWholeCell()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, EventIndicatorType = EventIndicatorType.BackgroundFull, EventIndicatorColor = Colors.Green,
			Events = new Plugin.Maui.Calendar.Models.EventCollection { [May(14)] = new List<string> { "Event" } },
		};

		Look(calendar, May(14)).FullColor.Should().Be(Colors.Green);
		Look(calendar, May(13)).FullColor.Should().Be(Colors.Transparent);
	}

	[Theory]
	[InlineData(EventIndicatorType.BottomDot)]
	[InlineData(EventIndicatorType.TopDot)]
	[InlineData(EventIndicatorType.Background)]
	[InlineData(EventIndicatorType.BackgroundFull)]
	public void Dots_ShowTheEventColors_ForEveryIndicatorType(EventIndicatorType type)
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, EventIndicatorType = type,
			Events = new Plugin.Maui.Calendar.Models.EventCollection { [May(14)] = new MultiColorEvents([Colors.Red, Colors.Green, Colors.Blue]) },
		};

		var dots = Look(calendar, May(14)).Dots;
		dots.Select(dot => dot.Color).Should().Equal(Colors.Red, Colors.Green, Colors.Blue);
		dots.Should().OnlyContain(dot => dot.Bounds.Width == 8 && dot.Bounds.Height == 8);
		((dots[0].Bounds.Left + dots[^1].Bounds.Right) / 2).Should().BeApproximately(Look(calendar, May(14)).Container.Center.X, 0.01f, "the dot row is centered");
		Look(calendar, May(13)).Dots.Should().BeEmpty();
	}

	[Fact]
	public void Dots_AreBelowTheNumber_AndAboveItForTopDot()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15,
			Events = new Plugin.Maui.Calendar.Models.EventCollection { [May(14)] = new List<string> { "Event" } },
		};

		Look(calendar, May(14)).Dots[0].Bounds.Top.Should().BeGreaterThan(Look(calendar, May(14)).TextBox.Bottom - 0.01f);

		calendar.EventIndicatorType = EventIndicatorType.TopDot;

		Look(calendar, May(14)).Dots[0].Bounds.Bottom.Should().BeLessThan(Look(calendar, May(14)).TextBox.Top + 0.01f);
	}

	[Fact]
	public void HiddenOtherMonthDays_KeepTheirPlace_AndHiddenWeeksTakeNoRoom()
	{
		var calendar = new TestCalendar { ShownDate = May15, OtherMonthDayIsVisible = false, OtherMonthWeekIsVisible = false };

		// May 2025 starts on a Thursday: April 27 shares the first week with May days, June 1 starts
		// a week of June days only.
		Look(calendar, new DateTime(2025, 4, 27)).Should().BeNull("the native DayView of an other-month day is hidden");
		Look(calendar, new DateTime(2025, 6, 1)).Should().BeNull("a week of other-month days takes no row");
		Drawable(calendar).CellBounds(35, width).Height.Should().Be(0);

		Look(calendar, May(14)).ShowsContent.Should().BeTrue();
	}

	[Fact]
	public void AWeekOfHiddenDays_TakesNoHeight()
	{
		var shown = new TestCalendar { ShownDate = May15 };
		var hidden = new TestCalendar { ShownDate = May15, OtherMonthWeekIsVisible = false };

		// May 2025 from Sunday: its sixth week (June 1-7) has no day of May.
		Drawable(hidden).CellBounds(35, width).Height.Should().Be(0);
		Grid(hidden).HeightRequest.Should().BeLessThan(Grid(shown).HeightRequest);
	}

	[Fact]
	public void OtherMonthDayIsVisibleFalse_WithVisibleWeeks_HidesTheDayInsideItsCell()
	{
		var calendar = new TestCalendar { ShownDate = May15, OtherMonthDayIsVisible = false, OtherMonthWeekIsVisible = true };

		var april27 = Look(calendar, new DateTime(2025, 4, 27));

		april27.Should().NotBeNull("the cell keeps its room");
		april27.ShowsContent.Should().BeFalse();
		april27.Text.Should().BeNull();
	}

	[Fact]
	public void DayLabel_UsesDaysLabelStyle()
	{
		var style = LabelStyle(Colors.Green);
		style.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 20d });
		style.Setters.Add(new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
		var calendar = new TestCalendar { ShownDate = May15, DaysLabelStyle = style };

		var look = Look(calendar, May(14));

		// Font families are not checked: on the net10.0 target of the tests there are no fonts.
		look.Font.Size.Should().Be(20);
		look.Font.Font.Weight.Should().Be(FontWeights.Bold);
		look.TextColor.Should().Be(look.Day.TextColor, "the day's state decides the color, not the style");
	}

	[Fact]
	public void DayLabel_StyleWithoutFontSize_UsesThePlatformLabelSize()
	{
		var calendar = new TestCalendar { ShownDate = May15, DaysLabelStyle = LabelStyle(Colors.Green) };

		Look(calendar, May(14)).Font.Size.Should().Be((float)DrawnLabelStyle.DefaultLabelFontSize);
	}

	[Fact]
	public void DayLabel_UseNativeDigits_ShowsTheNativeDigits()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, Culture = new System.Globalization.CultureInfo("ar-EG"), UseNativeDigits = true,
		};

		Look(calendar, May(14)).Text.Should().Be("١٤");
	}

	[Fact]
	public void DayLabel_IsCenteredInTheCell_AndItsMarginCountsTwice_AsInTheNativeFlexLayout()
	{
		var style = LabelStyle(Colors.Green);
		style.Setters.Add(new Setter { Property = View.MarginProperty, Value = new Thickness(0, 3, 0, 0) });
		var calendar = new TestCalendar { ShownDate = May15, DaysLabelStyle = style };
		var plain = new TestCalendar { ShownDate = May15, DaysLabelStyle = LabelStyle(Colors.Green) };

		var look = Look(calendar, May(14));
		look.TextBox.Center.X.Should().BeApproximately(look.Cell.Center.X, 0.01f);

		// The label is 2 × 3 higher; its text starts 2 × 3 below its top, and the column is centered.
		(look.TextBox.Top - Look(plain, May(14)).TextBox.Top).Should().BeApproximately(6 - 3, 0.01f);
	}

	// ── Weekend bands ────────────────────────────────────────────────────────

	[Fact]
	public void WeekendBands_NoneByDefault()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		Drawable(calendar).WeekendBands(width).Should().BeEmpty();
	}

	[Fact]
	public void WeekendBands_OnePerWeekendColumnAndWeek_ReachingHalfTheRowSpacingAboveAndBelow()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray, WeekendDayBackgroundCornerRadius = 10 };
		var drawable = Drawable(calendar);

		var bands = drawable.WeekendBands(width);

		bands.Should().HaveCount(12, "two weekend columns in six weeks");
		bands.Select(band => band.Column).Distinct().Should().BeEquivalentTo([0, 6], "Sunday and Saturday, Sunday first");
		bands.Should().OnlyContain(band => band.Color == Colors.LightGray && band.CornerRadius == 10);
		var week0 = drawable.CellBounds(0, width);
		bands[0].Bounds.Top.Should().BeApproximately(week0.Top - 3, 0.01f);
		bands[0].Bounds.Bottom.Should().BeApproximately(week0.Bottom + 3, 0.01f);
	}

	[Fact]
	public void WeekendBands_ArePaintedBehindTheDays()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray, SelectedDate = May(14) };
		var canvas = RecordingCanvas.Create();

		Drawable(calendar).Draw(canvas, new RectF(0, 0, width, (float)Grid(calendar).HeightRequest));

		var calls = RecordingCanvas.Calls(canvas);
		var bands = calls.Select((call, index) => (call, index)).Where(entry => entry.call.FillColor == Colors.LightGray && entry.call.Name == nameof(ICanvas.FillRoundedRectangle)).ToList();
		bands.Should().HaveCount(12);
		var firstText = calls.FindIndex(call => call.Name == nameof(ICanvas.DrawString));
		var selected = calls.FindIndex(call => call.Name == nameof(ICanvas.FillRoundedRectangle) && call.FillColor != Colors.LightGray);
		bands[^1].index.Should().BeLessThan(firstText).And.BeLessThan(selected, "the bands are painted first");
	}

	[Fact]
	public void WeekendBands_FollowFirstDayOfWeekAndTheLayout()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };

		calendar.FirstDayOfWeek = DayOfWeek.Monday;
		Drawable(calendar).WeekendBands(width).Select(band => band.Column).Distinct().Should().BeEquivalentTo([5, 6]);

		calendar.CalendarLayout = WeekLayout.Week;
		Drawable(calendar).WeekendBands(width).Should().HaveCount(2);
	}

	[Fact]
	public void WeekendBands_RemovedWhenTheColorIsCleared()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };

		calendar.WeekendDayBackgroundColor = Colors.Transparent;

		Drawable(calendar).WeekendBands(width).Should().BeEmpty();
	}

	// ── Painting ─────────────────────────────────────────────────────────────

	[Fact]
	public void TheGrid_CanBePainted()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15,
			SelectedDate = May15,
			WeekendDayBackgroundColor = Colors.LightGray,
			EventIndicatorType = EventIndicatorType.TopDot,
		};
		var grid = Grid(calendar);

		using var canvas = new PictureCanvas(0, 0, width, (float)grid.HeightRequest);
		var paint = () => grid.GridDrawable.Draw(canvas, new RectF(0, 0, width, (float)grid.HeightRequest));

		paint.Should().NotThrow();
	}

	[Fact]
	public void Painting_DrawsEveryShownNumberAndEveryTitle()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var canvas = RecordingCanvas.Create();

		Drawable(calendar).Draw(canvas, new RectF(0, 0, width, (float)Grid(calendar).HeightRequest));

		var texts = RecordingCanvas.Calls(canvas).Where(call => call.Name == nameof(ICanvas.DrawString)).Select(call => (string)call.Arguments[0]!).ToList();
		texts.Should().HaveCount(42 + 7);
		texts.Should().Contain("14").And.Contain("SUN");
	}

	// ── Swipes ───────────────────────────────────────────────────────────────

	[Fact]
	public void GridSwipe_ChangesTheMonth_AsTheCalendarsRecognizersDo()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var swipes = new List<string>();
		calendar.SwipedLeft += (_, _) => swipes.Add("left");

		calendar.OnDrawnDaySwiped(SwipeDirection.Left);

		swipes.Should().Equal("left");
		calendar.ShownDate.Month.Should().Be(6);
	}

	[Fact]
	public void GridSwipe_WhileSwipeDetectionIsDisabled_IsIgnored()
	{
		var calendar = new TestCalendar { ShownDate = May15, SwipeDetectionDisabled = true };

		calendar.OnDrawnDaySwiped(SwipeDirection.Left);
		calendar.ShownDate.Month.Should().Be(5);

		calendar.SwipeDetectionDisabled = false;
		calendar.OnDrawnDaySwiped(SwipeDirection.Left);
		calendar.ShownDate.Month.Should().Be(6, "the setting is read at the time of the swipe");
	}

	// ── Right to left ────────────────────────────────────────────────────────

	[Fact]
	public void RightToLeft_MirrorsTheColumns_AsTheNativeGridIsMirrored()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };
		var drawable = Drawable(calendar);

		calendar.FlowDirection = FlowDirection.RightToLeft;

		drawable.IsRightToLeft.Should().BeTrue();
		Titles(calendar).Select(title => ColumnOf(title.Box)).Should().Equal(6, 5, 4, 3, 2, 1, 0);
		Enumerable.Range(0, 7).Select(index => ColumnOf(drawable.CellBounds(index, width))).Should().Equal(6, 5, 4, 3, 2, 1, 0);
		drawable.CellBounds(0, width).Top.Should().Be(drawable.CellBounds(6, width).Top, "the first week stays the first row");
		drawable.WeekendBands(width).Select(band => band.Column).Distinct().Should().BeEquivalentTo([0, 6]);

		calendar.FlowDirection = FlowDirection.LeftToRight;

		Enumerable.Range(0, 7).Select(index => ColumnOf(drawable.CellBounds(index, width))).Should().Equal(0, 1, 2, 3, 4, 5, 6);
	}

	[Fact]
	public void RightToLeft_SetBeforeTheGridIsCreated_AndKeptAcrossLayouts()
	{
		var calendar = new TestCalendar { ShownDate = May15, FlowDirection = FlowDirection.RightToLeft };

		Enumerable.Range(0, 7).Select(index => ColumnOf(Drawable(calendar).CellBounds(index, width))).Should().Equal(6, 5, 4, 3, 2, 1, 0);

		calendar.CalendarLayout = WeekLayout.Week;
		calendar.FirstDayOfWeek = DayOfWeek.Monday;
		calendar.WeekendDayBackgroundColor = Colors.LightGray;

		Enumerable.Range(0, 7).Select(index => ColumnOf(Drawable(calendar).CellBounds(index, width))).Should().Equal(6, 5, 4, 3, 2, 1, 0);
		Drawable(calendar).WeekendBands(width).Select(band => band.Column).Should().BeEquivalentTo([1, 0], "Saturday and Sunday are the last days, on the left");
	}

	// ── Titles ───────────────────────────────────────────────────────────────

	[Fact]
	public void Titles_FollowFirstDayOfWeekAndTheTitleOptions()
	{
		var calendar = new TestCalendar { ShownDate = May15, FirstDayOfWeek = DayOfWeek.Monday };

		calendar.DaysTitleLabelFirstUpperRestLower = true;

		Titles(calendar).Select(title => title.Text).Should().Equal("Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun");
	}

	[Fact]
	public void Titles_UseDaysTitleLabelStyle_AndWeekendTitleStyleOnWeekendDays()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.DaysTitleLabelStyle = LabelStyle(Colors.Green);
		calendar.WeekendTitleStyle = LabelStyle(Colors.Red);

		// Sunday first: the first and the last column are weekend days.
		Titles(calendar).Select(title => title.Color)
			.Should().Equal(Colors.Red, Colors.Green, Colors.Green, Colors.Green, Colors.Green, Colors.Green, Colors.Red);
	}

	[Fact]
	public void Titles_FillTheirColumn_WhateverTheAlignmentOfTheirStyle()
	{
		// The native titles are given their alignment by the grid, which a style cannot change: they are
		// drawn centered in their column (less the label's margin and padding).
		var start = LabelStyle(Colors.Green);
		start.Setters.Add(new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start });
		var calendar = new TestCalendar { ShownDate = May15, WeekendTitleStyle = start, DaysTitleLabelStyle = LabelStyle(Colors.Green) };

		Titles(calendar).Should().OnlyContain(title => Math.Abs(title.Box.Width - (width / 7)) < 0.01f);
	}

	[Fact]
	public void Titles_FollowTheCultureAndTheLengthOptions()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.Culture = new System.Globalization.CultureInfo("de-DE");
		calendar.DaysTitleMaximumLength = DaysTitleMaxLength.TwoChars;

		Titles(calendar).Select(title => title.Text).Should().Equal("SO", "MO", "DI", "MI", "DO", "FR", "SA");
	}

	// ── Accessibility ────────────────────────────────────────────────────────

	// The grid's screen reader elements once it is laid out (they are where the days are drawn).
	static IReadOnlyList<GraphicsGridElement> Elements(CalendarControl calendar)
	{
		var grid = Grid(calendar);
		grid.Frame = new Rect(0, 0, width, grid.HeightRequest);
		return grid.AccessibleElements();
	}

	static GraphicsGridElement Element(CalendarControl calendar, DateTime date) =>
		Elements(calendar).Single(element => element.IsDay && element.Id == IndexOf(calendar, date));

	[Fact]
	public void Accessibility_EveryShownDayIsAButton_NamedWithItsFullDate()
	{
		var calendar = new TestCalendar { ShownDate = May15, Culture = new System.Globalization.CultureInfo("en-US") };

		var day = Element(calendar, May(14));

		day.Label.Should().Be("Wednesday, May 14, 2025");
		day.IsEnabled.Should().BeTrue();
		day.IsSelected.Should().BeFalse();
		Elements(calendar).Count(element => element.IsDay).Should().Be(42);
	}

	[Fact]
	public void Accessibility_FollowsTheCalendarCulture()
	{
		var calendar = new TestCalendar { ShownDate = May15, Culture = new System.Globalization.CultureInfo("en-US") };

		calendar.Culture = new System.Globalization.CultureInfo("de-DE");

		Element(calendar, May(14)).Label.Should().Be("Mittwoch, 14. Mai 2025");
	}

	[Fact]
	public void Accessibility_SelectedDayIsSelected_DisabledDayCannotBeUsed()
	{
		var calendar = new TestCalendar { ShownDate = May15, SelectedDate = May(14), DisabledDates = [May(20)] };

		Element(calendar, May(14)).IsSelected.Should().BeTrue();
		Element(calendar, May(20)).IsEnabled.Should().BeFalse("a disabled day is still read, as unavailable");
	}

	[Fact]
	public void Accessibility_HiddenDays_AreLeftOut()
	{
		var calendar = new TestCalendar { ShownDate = May15, OtherMonthDayIsVisible = false };

		Elements(calendar).Should().NotContain(element => element.Label.Contains("April"));
		Elements(calendar).Count(element => element.IsDay).Should().Be(31);
	}

	[Fact]
	public void Accessibility_TitlesAreTextFirst_InTheFirstRow()
	{
		var calendar = new TestCalendar { ShownDate = May15, Culture = new System.Globalization.CultureInfo("en-US") };

		var titles = Elements(calendar).Take(7).ToList();

		titles.Should().OnlyContain(element => !element.IsDay && element.Row == 0);
		titles.Select(element => element.Column).Should().Equal(0, 1, 2, 3, 4, 5, 6);
		titles.Select(element => element.Label).Should().Equal(Drawable(calendar).ShownTitles);
		titles[0].Label.Should().Be("SUN", "a title is read as it is shown (the default title style upper-cases it)");
	}

	[Fact]
	public void Accessibility_ElementsAreWhereTheDaysAreDrawn()
	{
		var calendar = new TestCalendar { ShownDate = May15, FlowDirection = FlowDirection.RightToLeft };
		var drawable = Drawable(calendar);

		foreach (var element in Elements(calendar).Where(element => element.IsDay))
		{
			element.Bounds.Should().Be(drawable.CellBounds(element.Id, width));
			element.Row.Should().Be(1 + (element.Id / 7));
		}
	}

	[Fact]
	public void Accessibility_ActivatingADay_TapsIt_AndATitleOrADisabledDayDoesNothing()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var grid = Grid(calendar);

		calendar.WithTapHandling(() => grid.Activate(IndexOf(calendar, May(20))).Should().BeTrue());
		calendar.SelectedDate.Should().Be(May(20));

		grid.Activate(GraphicsDayGrid.TitleId).Should().BeFalse();
	}

	// ── Helpers ──────────────────────────────────────────────────────────────

	sealed class MultiColorEvents(IReadOnlyList<Color> colors) : List<string>, IMultiEventDay
	{
		public IReadOnlyList<Color> Colors { get; } = colors;
	}

	sealed class FixedTimeProvider(DateTime now) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => new(now, TimeSpan.Zero);

		public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
	}

	static Style LabelStyle(Color textColor)
	{
		var style = new Style(typeof(Label));
		style.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = textColor });
		return style;
	}
}

/// <summary>
/// A canvas that records what is drawn on it: every method called, with its arguments and the fill
/// color at the time. Property setters other than FillColor are ignored.
/// </summary>
public class RecordingCanvas : DispatchProxy
{
	public sealed record Call(string Name, object?[] Arguments, Color? FillColor);

	readonly List<Call> calls = [];
	Color? fillColor;

	public static ICanvas Create() => Create<ICanvas, RecordingCanvas>();

	public static List<Call> Calls(ICanvas canvas) => ((RecordingCanvas)(object)canvas).calls;

	protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
	{
		if (targetMethod is null)
		{
			return null;
		}

		if (targetMethod.Name == "set_FillColor")
		{
			fillColor = (Color?)args![0];
			return null;
		}

		if (!targetMethod.Name.StartsWith("set_", StringComparison.Ordinal) && !targetMethod.Name.StartsWith("get_", StringComparison.Ordinal))
		{
			calls.Add(new Call(targetMethod.Name, args ?? [], fillColor));
		}

		// GetStringSize is the only drawing method with a result that matters: a size of nothing.
		return targetMethod.ReturnType.IsValueType && targetMethod.ReturnType != typeof(void)
			? Activator.CreateInstance(targetMethod.ReturnType)
			: null;
	}
}
