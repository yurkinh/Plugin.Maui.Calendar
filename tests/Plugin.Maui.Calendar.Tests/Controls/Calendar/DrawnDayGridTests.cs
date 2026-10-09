using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls;
using Plugin.Maui.Calendar.Controls.Drawn;
using DrawnUi.Draw;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Interfaces;
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

	[Fact]
	public void DayViewTemplateSelector_UsesTheNativeGrid_AndItsChoices()
	{
		var label = CalendarTestExtensions.DayLabelTemplate();
		var selector = new RecordingTemplateSelector(day => day.IsSelected ? label : null);

		var calendar = new TestCalendar { ShownDate = May15, SelectedDate = May(14), DayViewTemplate = selector };
		calendar.EnsureAllContent();

		calendar.DrawnDayGrid.Should().BeNull();
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

		var grid = calendar.DrawnDayGrid!;
		grid.Cells.Select(cell => cell.Day).Should().Equal(calendar.Days(), "the drawn grid shows the new days");
		var day14 = grid.Cells.Single(cell => cell.Day.Date == May(14)).Day;
		day14.IsSelected.Should().BeTrue();
		day14.BackgroundColor.Should().Be(Colors.LightBlue, "the range colors are applied to the new days");
		grid.Cells.Single(cell => cell.Day.Date == May(20)).Day.HasEvents.Should().BeTrue();
		calendar.ShownDate.Month.Should().Be(5);
	}

	[Fact]
	public void SwitchingToTheNativeGrid_ReleasesTheDrawnDays()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var grid = calendar.DrawnDayGrid!;

		calendar.DayViewTemplate = CalendarTestExtensions.DayLabelTemplate();

		grid.Cells.Should().OnlyContain(cell => cell.Day == null, "the drawn cells no longer follow days the calendar replaced");
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
		calendar.DrawnDayGrid.Should().BeNull();
		calendar.DayViews.Should().OnlyContain(dayView => dayView.ShowsTemplateContent());
		calendar.Named<Grid>("daysControl").Children.OfType<DayView>().Should().HaveCount(42);
	}

	[Fact]
	public void DrawnGrid_IsCreatedWhenTheCalendarGetsAHandler()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };
		calendar.IsDrawnDayGridCreated.Should().BeFalse("nothing is drawn before the calendar is shown");

		calendar.SimulateHandlerAttached();

		calendar.IsDrawnDayGridCreated.Should().BeTrue();
		var grid = calendar.DrawnDayGrid!;
		grid.Cells.Select(cell => cell.Day).Should().Equal(calendar.Days());
		grid.TitleLabels[1].Text.Should().Be("MON");
		grid.WeekendBands.Should().HaveCount(12);
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

	// ── Default cell ─────────────────────────────────────────────────────────

	static DrawnDayCell Cell(CalendarControl calendar, DateTime date) =>
		DrawnGrid(calendar).Cells.Single(cell => cell.Day.Date == date);

	[Fact]
	public void Cell_FillsItsColumn_WithADayViewSizeByDayViewHeightContainer()
	{
		var calendar = new TestCalendar { ShownDate = May15, DayViewSize = 32, DayViewHeight = 56 };

		var cell = Cell(calendar, May(14));

		cell.HorizontalOptions.Should().Be(LayoutOptions.Fill);
		cell.HeightRequest.Should().Be(56);
		cell.Container.WidthRequest.Should().Be(32);
		cell.Container.HeightRequest.Should().Be(56);
	}

	[Fact]
	public void Background_FillsTheContainerUpToDayViewSize_InsideDayViewBorderMargin()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, DayViewSize = 40, DayViewHeight = 60, DayViewBorderMargin = new Thickness(2, 3), DayViewCornerRadius = 6,
		};

		var cell = Cell(calendar, May(14));

		// Width: 40 less the horizontal margin; height: the native Border's maximum, DayViewSize.
		cell.BackgroundShape.WidthRequest.Should().Be(36);
		cell.BackgroundShape.HeightRequest.Should().Be(40);
		cell.BackgroundShape.Margin.Should().Be(new Thickness(2, 3));
		cell.BackgroundShape.CornerRadius.Should().Be(new CornerRadius(6));
		cell.Outline.WidthRequest.Should().Be(36);
		cell.Outline.CornerRadius.Should().Be(6);
	}

	[Fact]
	public void Outline_ShowsTheTodayOutline_OnlyOnTodayWhileNotSelected()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, TodayOutlineColor = Colors.Green, TimeProvider = new FixedTimeProvider(May(14)),
		};

		Cell(calendar, May(14)).Outline.Color.Should().Be(Colors.Green);
		Cell(calendar, May(15)).Outline.Color.Should().Be(Colors.Transparent);

		calendar.SelectedDate = May(14);

		Cell(calendar, May(14)).Outline.Color.Should().Be(Colors.Transparent);
	}

	[Fact]
	public void BackgroundFull_PaintsTheWholeCell()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, EventIndicatorType = EventIndicatorType.BackgroundFull, EventIndicatorColor = Colors.Green,
			Events = new Plugin.Maui.Calendar.Models.EventCollection { [May(14)] = new List<string> { "Event" } },
		};

		Cell(calendar, May(14)).BackgroundColor.Should().Be(Colors.Green);
		Cell(calendar, May(13)).BackgroundColor.Should().Be(Colors.Transparent);
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

		Cell(calendar, May(14)).Dots.Where(dot => dot.IsVisible).Select(dot => dot.BackgroundColor)
			.Should().Equal(Colors.Red, Colors.Green, Colors.Blue);
		Cell(calendar, May(13)).Dots.Should().OnlyContain(dot => !dot.IsVisible);
	}

	[Fact]
	public void HiddenOtherMonthDays_KeepTheirPlace_AndHiddenWeeksTakeNoRoom()
	{
		var calendar = new TestCalendar { ShownDate = May15, OtherMonthDayIsVisible = false, OtherMonthWeekIsVisible = false };

		// May 2025 starts on a Thursday: April 27 shares the first week with May days, June 1 starts
		// a week of June days only.
		var april27 = Cell(calendar, new DateTime(2025, 4, 27));
		april27.IsVisible.Should().BeFalse("the native DayView of an other-month day is hidden");

		var june1 = Cell(calendar, new DateTime(2025, 6, 1));
		june1.IsVisible.Should().BeFalse("a week of other-month days takes no row");

		Cell(calendar, May(14)).IsVisible.Should().BeTrue();
		Cell(calendar, May(14)).Container.Opacity.Should().Be(1);
	}

	[Fact]
	public void OtherMonthDayIsVisibleFalse_WithVisibleWeeks_HidesTheDayInsideItsCell()
	{
		var calendar = new TestCalendar { ShownDate = May15, OtherMonthDayIsVisible = false, OtherMonthWeekIsVisible = true };

		var april27 = Cell(calendar, new DateTime(2025, 4, 27));

		april27.IsVisible.Should().BeTrue();
		april27.Container.Opacity.Should().Be(0);
	}

	[Fact]
	public void DayLabel_UsesDaysLabelStyle()
	{
		var style = LabelStyle(Colors.Green);
		style.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 20d });
		style.Setters.Add(new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold });
		var calendar = new TestCalendar { ShownDate = May15, DaysLabelStyle = style };

		var cell = Cell(calendar, May(14));
		var label = cell.Label;

		// Font families are not checked: on the net10.0 target of the tests there are no fonts.
		label.FontSize.Should().Be(20);
		label.FontAttributes.Should().Be(FontAttributes.Bold);
		label.TextColor.Should().Be(cell.Day.TextColor, "the day's state decides the color, not the style");
	}

	[Fact]
	public void DayLabel_StyleWithoutFontSize_UsesThePlatformLabelSize()
	{
		var calendar = new TestCalendar { ShownDate = May15, DaysLabelStyle = LabelStyle(Colors.Green) };

		Cell(calendar, May(14)).Label.FontSize.Should().Be(DrawnText.DefaultLabelFontSize);
	}

	[Fact]
	public void DayLabel_UseNativeDigits_ShowsTheNativeDigits()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15, Culture = new System.Globalization.CultureInfo("ar-EG"), UseNativeDigits = true,
		};

		Cell(calendar, May(14)).Label.Text.Should().Be("١٤");
	}

	// ── Weekend bands ────────────────────────────────────────────────────────

	[Fact]
	public void WeekendBands_NoneByDefault()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		DrawnGrid(calendar).WeekendBands.Should().BeEmpty();
	}

	[Fact]
	public void WeekendBands_OnePerWeekendColumnAndWeek_BehindTheCells()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray, WeekendDayBackgroundCornerRadius = 10 };

		var grid = DrawnGrid(calendar);
		grid.WeekendBands.Should().HaveCount(12, "two weekend columns in six weeks");
		grid.WeekendBands.Select(Grid.GetColumn).Distinct().Should().BeEquivalentTo([0, 6], "Sunday and Saturday, Sunday first");
		grid.WeekendBands.Should().OnlyContain(band =>
			band.BackgroundColor == Colors.LightGray && band.CornerRadius == new CornerRadius(10) && band.InputTransparent
			&& band.Margin == new Thickness(0, -3, 0, -3));

		var children = grid.Views.ToList();
		children.IndexOf(grid.WeekendBands[^1]).Should().BeLessThan(children.IndexOf(grid.Cells[0]), "the bands are drawn first");
	}

	[Fact]
	public void WeekendBands_FollowFirstDayOfWeekAndTheLayout()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };

		calendar.FirstDayOfWeek = DayOfWeek.Monday;
		DrawnGrid(calendar).WeekendBands.Select(Grid.GetColumn).Distinct().Should().BeEquivalentTo([5, 6]);

		calendar.CalendarLayout = WeekLayout.Week;
		DrawnGrid(calendar).WeekendBands.Should().HaveCount(2);
	}

	[Fact]
	public void WeekendBands_RemovedWhenTheColorIsCleared()
	{
		var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };
		var band = DrawnGrid(calendar).WeekendBands[0];

		calendar.WeekendDayBackgroundColor = Colors.Transparent;

		DrawnGrid(calendar).WeekendBands.Should().BeEmpty();
		DrawnGrid(calendar).Views.Should().NotContain(band);
	}

	[Fact]
	public void IndicatorTypeChange_KeepsTheCellContent()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var label = Cell(calendar, May(14)).Label;

		calendar.EventIndicatorType = EventIndicatorType.TopDot;
		calendar.EventIndicatorType = EventIndicatorType.BottomDot;

		// DrawnUI disposes a removed child after a delay, but marks it at once.
		label.IsAlive.Should().NotBe(ObjectAliveType.BeingDisposed, "moving the dot row must not dispose the label");
		Cell(calendar, May(14)).Label.Should().BeSameAs(label);
	}

	// ── Input ────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData(-120, 0, SwipeDirection.Left)]
	[InlineData(120, 10, SwipeDirection.Right)]
	[InlineData(30, -150, SwipeDirection.Up)]
	[InlineData(-40, 101, SwipeDirection.Down)]
	[InlineData(100, -100, SwipeDirection.Right)]
	public void GetSwipe_TheLargerMovementDecides(float x, float y, SwipeDirection expected)
	{
		DrawnDayGrid.GetSwipe(x, y).Should().Be(expected);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(-99, 0)]
	[InlineData(60, 80)]
	public void GetSwipe_ShorterThanTheThreshold_IsNoSwipe(float x, float y)
	{
		DrawnDayGrid.GetSwipe(x, y).Should().BeNull();
	}

	[Fact]
	public void SwipeThreshold_IsTheOneOfMauisSwipeRecognizer()
	{
		DrawnDayGrid.SwipeThreshold.Should().Be((float)new SwipeGestureRecognizer().Threshold);
	}

	[Fact]
	public void DrawnSwipe_ChangesTheMonth_AsTheCalendarsRecognizersDo()
	{
		var calendar = new TestCalendar { ShownDate = May15 };
		var swipes = new List<string>();
		calendar.SwipedLeft += (_, _) => swipes.Add("left");

		calendar.OnDrawnDaySwiped(SwipeDirection.Left);

		swipes.Should().Equal("left");
		calendar.ShownDate.Month.Should().Be(6);
	}

	[Fact]
	public void DrawnSwipe_WhileSwipeDetectionIsDisabled_IsIgnored()
	{
		var calendar = new TestCalendar { ShownDate = May15, SwipeDetectionDisabled = true };

		calendar.OnDrawnDaySwiped(SwipeDirection.Left);
		calendar.ShownDate.Month.Should().Be(5);

		calendar.SwipeDetectionDisabled = false;
		calendar.OnDrawnDaySwiped(SwipeDirection.Left);
		calendar.ShownDate.Month.Should().Be(6, "the setting is read at the time of the swipe");
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

	[Fact]
	public void Titles_AreCentered_UnlessTheirStyleSaysOtherwise()
	{
		var start = LabelStyle(Colors.Green);
		start.Setters.Add(new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start });
		var calendar = new TestCalendar { ShownDate = May15, WeekendTitleStyle = start };

		DrawnGrid(calendar).TitleLabels.Select(label => label.HorizontalTextAlignment).Should().Equal(
			DrawTextAlignment.Start, DrawTextAlignment.Center, DrawTextAlignment.Center, DrawTextAlignment.Center,
			DrawTextAlignment.Center, DrawTextAlignment.Center, DrawTextAlignment.Start);
	}

	[Fact]
	public void Titles_FollowTheCultureAndTheLengthOptions()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.Culture = new System.Globalization.CultureInfo("de-DE");
		calendar.DaysTitleMaximumLength = DaysTitleMaxLength.TwoChars;

		DrawnGrid(calendar).TitleLabels.Select(label => label.Text)
			.Should().Equal("SO", "MO", "DI", "MI", "DO", "FR", "SA");
	}

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
