using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Controls.Drawn;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies that the texts of the grid drawn with Microsoft.Maui.Graphics get their styles as the native
/// labels do: based-on styles, the app's implicit Label style, dynamic resources and theme values, and
/// their changes while the calendar is shown.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public sealed class GraphicsLabelStyleTests : IDisposable
{
	static readonly DateTime May15 = new(2025, 5, 15);

	readonly Application? previousApplication = Application.Current;

	const float width = 350;

	public GraphicsLabelStyleTests() => AppContext.SetSwitch(CalendarControl.GraphicsDayGridSwitch, true);

	public void Dispose()
	{
		AppContext.SetSwitch(CalendarControl.GraphicsDayGridSwitch, false);
		Application.Current = previousApplication;
	}

	static GraphicsDayGridDrawable Drawable(CalendarControl calendar) =>
		(calendar.GraphicsDayGrid ?? throw new InvalidOperationException("The calendar does not show the Microsoft.Maui.Graphics grid.")).GridDrawable;

	// Sunday first: the first and the last title are weekend days.
	static GraphicsDayGridDrawable.TitleLook WeekdayTitle(CalendarControl calendar) => Drawable(calendar).TitleLookOf(1, width);

	static Color WeekdayTitleColor(CalendarControl calendar) => WeekdayTitle(calendar).Color;

	static IEnumerable<GraphicsDayGridDrawable.DayLook> Cells(CalendarControl calendar) =>
		Enumerable.Range(0, Drawable(calendar).Days.Count).Select(index => Drawable(calendar).DayLookOf(index, width));

	static GraphicsDayGridDrawable.DayLook Cell(CalendarControl calendar) => Cells(calendar).Single(cell => cell.Day.Date == May15);

	static Style Style(params (BindableProperty Property, object Value)[] setters)
	{
		var style = new Style(typeof(Label));
		foreach (var (property, value) in setters)
		{
			style.Setters.Add(new Setter { Property = property, Value = value });
		}
		return style;
	}

	[Fact]
	public void BasedOnStyles_AreResolved()
	{
		var baseStyle = Style((Label.TextColorProperty, Colors.Green), (Label.FontSizeProperty, 21d));
		var style = Style((Label.FontAttributesProperty, FontAttributes.Bold));
		style.BasedOn = baseStyle;

		var calendar = new TestCalendar { ShownDate = May15, DaysTitleLabelStyle = style };

		var title = WeekdayTitle(calendar);
		title.Color.Should().Be(Colors.Green);
		title.Font.Size.Should().Be(21);
		title.Font.Font.Weight.Should().Be(FontWeights.Bold);
	}

	[Fact]
	public void ImplicitLabelStyleOfThePage_AppliesUnderTheCalendarStyle()
	{
		var calendar = new TestCalendar { ShownDate = May15, DaysTitleLabelStyle = Style((Label.FontSizeProperty, 12d)) };
		var page = new ContentPage { Content = calendar };

		page.Resources.Add(Style((Label.TextColorProperty, Colors.Teal), (Label.CharacterSpacingProperty, 2d)));

		WeekdayTitleColor(calendar).Should().Be(Colors.Teal, "the calendar style sets no text color");
		WeekdayTitle(calendar).Font.Size.Should().Be(12, "the calendar style wins over the implicit one");
		Cell(calendar).CharacterSpacing.Should().Be(2);
	}

	[Fact]
	public void ImplicitLabelStyleOfTheApp_Applies()
	{
		var app = new Application();
		app.Resources.Add(Style((Label.CharacterSpacingProperty, 3d)));
		Application.Current = app;

		var calendar = new TestCalendar { ShownDate = May15 };
		_ = new ContentPage { Content = calendar, Parent = app };

		Cell(calendar).CharacterSpacing.Should().Be(3);
	}

	[Fact]
	public void DynamicResources_AreResolved_AndFollowedWhenTheyChange()
	{
		var calendar = new TestCalendar
		{
			ShownDate = May15,
			DaysTitleLabelStyle = Style((Label.TextColorProperty, new DynamicResource("TitleColor"))),
		};
		var page = new ContentPage { Content = calendar };

		page.Resources["TitleColor"] = Colors.Red;
		WeekdayTitleColor(calendar).Should().Be(Colors.Red);

		page.Resources["TitleColor"] = Colors.Blue;
		WeekdayTitleColor(calendar).Should().Be(Colors.Blue);
	}

	[Fact]
	public void ThemeValues_FollowTheAppTheme()
	{
		var app = new Application();
		Application.Current = app;
		var resources = new ResourceDictionary().LoadFromXaml("""
			<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			    <Style x:Key="Title" TargetType="Label">
			        <Setter Property="TextColor" Value="{AppThemeBinding Light=Red, Dark=Blue}" />
			    </Style>
			</ResourceDictionary>
			""");
		app.UserAppTheme = AppTheme.Light;

		var calendar = new TestCalendar { ShownDate = May15, DaysTitleLabelStyle = (Style)resources["Title"] };
		_ = new ContentPage { Content = calendar, Parent = app };
		WeekdayTitleColor(calendar).Should().Be(Colors.Red);

		app.UserAppTheme = AppTheme.Dark;

		WeekdayTitleColor(calendar).Should().Be(Colors.Blue);
	}

	[Fact]
	public void NoTextColorInAnyStyle_UsesThePlatformTextColor_OfTheCurrentTheme()
	{
		var app = new Application { UserAppTheme = AppTheme.Light };
		Application.Current = app;
		var calendar = new TestCalendar { ShownDate = May15, DaysTitleLabelStyle = Style((Label.FontSizeProperty, 15d)) };
		WeekdayTitleColor(calendar).Should().Be(Colors.Black);

		app.UserAppTheme = AppTheme.Dark;

		WeekdayTitleColor(calendar).Should().Be(Colors.White);
	}

	[Fact]
	public void NoCharacterSpacing_DrawsTheNormalSpacing()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		Cell(calendar).CharacterSpacing.Should().Be(0);
		Enumerable.Range(0, 7).Should().OnlyContain(column => Drawable(calendar).TitleLookOf(column, width).CharacterSpacing == 0);
	}

	[Fact]
	public void NewDaysLabelStyle_IsShownByEveryCell()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.DaysLabelStyle = Style((Label.FontSizeProperty, 22d), (Label.TextTransformProperty, TextTransform.Uppercase));

		Cells(calendar).Should().OnlyContain(cell => cell.Font.Size == 22);

		// Day numbers have no case: the transform is seen with letters, as a culture's own digits are not.
		calendar.DaysLabelStyle = Style((Label.TextTransformProperty, TextTransform.Lowercase));
		Cell(calendar).Text.Should().Be("15");
		Cells(calendar).Should().OnlyContain(cell => cell.Font.Size == (float)DrawnLabelStyle.DefaultLabelFontSize, "the new style sets no size");
	}

	[Fact]
	public void DayLabelColor_ComesFromTheDay_EvenWhenTheStyleSetsOne()
	{
		var calendar = new TestCalendar { ShownDate = May15, DaysLabelStyle = Style((Label.TextColorProperty, Colors.Orange)) };

		Cell(calendar).TextColor.Should().Be(Cell(calendar).Day.TextColor);

		calendar.DaysLabelStyle = Style((Label.TextColorProperty, Colors.Purple));

		Cell(calendar).TextColor.Should().Be(Cell(calendar).Day.TextColor);
	}

	[Fact]
	public void TextTransformOfTheTitleStyle_IsDrawn()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.DaysTitleLabelStyle = Style((Label.TextTransformProperty, TextTransform.Lowercase));

		Drawable(calendar).TitleLookOf(1, width).Text.Should().Be("mon");
	}
}
