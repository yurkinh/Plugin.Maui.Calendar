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
/// Verifies that the drawn labels get their styles as the native labels do (DrawnUI migration,
/// phase 3): based-on styles, the app's implicit Label style, dynamic resources and theme values,
/// and their changes while the calendar is shown.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public sealed class DrawnLabelStyleTests : IDisposable
{
	static readonly DateTime May15 = new(2025, 5, 15);

	readonly Application? previousApplication = Application.Current;

	public DrawnLabelStyleTests() => AppContext.SetSwitch(CalendarControl.DrawnDayGridSwitch, true);

	public void Dispose()
	{
		AppContext.SetSwitch(CalendarControl.DrawnDayGridSwitch, false);
		Application.Current = previousApplication;
	}

	static DrawnDayGrid DrawnGrid(CalendarControl calendar) =>
		calendar.DrawnDayGrid ?? throw new InvalidOperationException("The calendar shows the native grid.");

	// Sunday first: the first and the last title are weekend days.
	static Color WeekdayTitleColor(CalendarControl calendar) => DrawnGrid(calendar).TitleLabels[1].TextColor;

	static DrawnDayCell Cell(CalendarControl calendar) => DrawnGrid(calendar).Cells.Single(cell => cell.Day.Date == May15);

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

		var title = DrawnGrid(calendar).TitleLabels[1];
		title.TextColor.Should().Be(Colors.Green);
		title.FontSize.Should().Be(21);
		title.FontAttributes.Should().Be(FontAttributes.Bold);
	}

	[Fact]
	public void ImplicitLabelStyleOfThePage_AppliesUnderTheCalendarStyle()
	{
		var calendar = new TestCalendar { ShownDate = May15, DaysTitleLabelStyle = Style((Label.FontSizeProperty, 12d)) };
		var page = new ContentPage { Content = calendar };

		page.Resources.Add(Style((Label.TextColorProperty, Colors.Teal), (Label.CharacterSpacingProperty, 2d)));

		WeekdayTitleColor(calendar).Should().Be(Colors.Teal, "the calendar style sets no text color");
		DrawnGrid(calendar).TitleLabels[1].FontSize.Should().Be(12, "the calendar style wins over the implicit one");
		// A SkiaLabel's normal spacing is 1, a native label's 0.
		Cell(calendar).Label.CharacterSpacing.Should().Be(1 + 2);
	}

	[Fact]
	public void ImplicitLabelStyleOfTheApp_Applies()
	{
		var app = new Application();
		app.Resources.Add(Style((Label.CharacterSpacingProperty, 3d)));
		Application.Current = app;

		var calendar = new TestCalendar { ShownDate = May15 };
		_ = new ContentPage { Content = calendar, Parent = app };

		Cell(calendar).Label.CharacterSpacing.Should().Be(1 + 3);
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
	public void NoCharacterSpacing_IsTheNormalSpacingOfASkiaLabel()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		Cell(calendar).Label.CharacterSpacing.Should().Be(1);
		DrawnGrid(calendar).TitleLabels.Should().OnlyContain(label => label.CharacterSpacing == 1);
	}

	[Fact]
	public void NewDaysLabelStyle_IsShownByEveryCell()
	{
		var calendar = new TestCalendar { ShownDate = May15 };

		calendar.DaysLabelStyle = Style((Label.FontSizeProperty, 22d), (Label.TextTransformProperty, TextTransform.Uppercase));

		DrawnGrid(calendar).Cells.Should().OnlyContain(cell => cell.Label.FontSize == 22);
		Cell(calendar).Label.TextTransform.Should().Be(DrawnUi.Draw.TextTransform.Uppercase);
	}

	[Fact]
	public void DayLabelColor_ComesFromTheDay_EvenWhenTheStyleSetsOne()
	{
		var calendar = new TestCalendar { ShownDate = May15, DaysLabelStyle = Style((Label.TextColorProperty, Colors.Orange)) };

		Cell(calendar).Label.TextColor.Should().Be(Cell(calendar).Day.TextColor);

		calendar.DaysLabelStyle = Style((Label.TextColorProperty, Colors.Purple));

		Cell(calendar).Label.TextColor.Should().Be(Cell(calendar).Day.TextColor);
	}
}
