using System.Globalization;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies the row of weekday titles above the days and the weekend columns: the title texts for
/// the title options, the weekend title style and the weekend background boxes.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class DayTitlesAndWeekendTests
{
    static readonly DateTime May15 = new(2025, 5, 15);

    static Grid Days(CalendarControl calendar) => calendar.Named<Grid>("daysControl");

    static List<Label> Titles(CalendarControl calendar) =>
        [.. Days(calendar).Children.OfType<Label>().OrderBy(Grid.GetColumn)];

    static List<string> TitleTexts(CalendarControl calendar) => [.. Titles(calendar).Select(label => label.Text)];

    static List<Border> WeekendBoxes(CalendarControl calendar) =>
        [.. Days(calendar).Children.OfType<Border>().Where(border => border.InputTransparent)];

    // ── Titles ───────────────────────────────────────────────────────────────

    [Fact]
    public void Titles_DefaultToThreeUpperCaseCharactersFromFirstDayOfWeek()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        TitleTexts(calendar).Should().Equal("SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT");

        calendar.FirstDayOfWeek = DayOfWeek.Monday;
        TitleTexts(calendar).Should().Equal("MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN");
    }

    [Fact]
    public void Titles_MaximumLengthAndLetterCase_FollowTheirProperties()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.DaysTitleMaximumLength = DaysTitleMaxLength.OneChar;
        TitleTexts(calendar)[0].Should().Be("S");

        calendar.DaysTitleMaximumLength = DaysTitleMaxLength.None;
        TitleTexts(calendar)[0].Should().Be("SUNDAY");

        calendar.DaysTitleLabelFirstUpperRestLower = true;
        TitleTexts(calendar)[0].Should().Be("Sunday");
    }

    [Fact]
    public void Titles_UseAbbreviatedDayNames_UsesTheCulturesAbbreviationsAndIgnoresTheMaximumLength()
    {
        var calendar = new TestCalendar
        {
            ShownDate = May15,
            Culture = new CultureInfo("uk-UA"),
            DaysTitleMaximumLength = DaysTitleMaxLength.OneChar,
            DaysTitleLabelFirstUpperRestLower = true,
        };

        calendar.UseAbbreviatedDayNames = true;

        TitleTexts(calendar)[1].Should().Be(new CultureInfo("uk-UA").DateTimeFormat.AbbreviatedDayNames[1].Capitalize());
    }

    [Fact]
    public void Titles_ArabicDayNames_LoseTheirDefiniteArticle()
    {
        var calendar = new TestCalendar { ShownDate = May15, Culture = new CultureInfo("ar-EG"), DaysTitleMaximumLength = DaysTitleMaxLength.None };

        TitleTexts(calendar).Should().AllSatisfy(title => title.Should().NotStartWith("ال"));
    }

    [Fact]
    public void Titles_WeekendTitleStyle_AppliesToSaturdayAndSundayAndFollowsRuntimeChanges()
    {
        var weekendStyle = new Style(typeof(Label));
        var calendar = new TestCalendar { ShownDate = May15, FirstDayOfWeek = DayOfWeek.Monday };

        calendar.WeekendTitleStyle = weekendStyle;

        var titles = Titles(calendar);
        titles[5].Style.Should().BeSameAs(weekendStyle);
        titles[6].Style.Should().BeSameAs(weekendStyle);
        titles[0].Style.Should().BeSameAs(calendar.DaysTitleLabelStyle);
    }

    [Fact]
    public void Titles_DaysTitleLabelStyle_IsBoundToEveryWeekdayTitle()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var style = new Style(typeof(Label));

        calendar.DaysTitleLabelStyle = style;

        Titles(calendar)[1].Style.Should().BeSameAs(style);
    }

    // ── Weekend background ───────────────────────────────────────────────────

    [Fact]
    public void WeekendBackground_TransparentByDefault_AddsNothing()
    {
        WeekendBoxes(new TestCalendar { ShownDate = May15 }).Should().BeEmpty();
    }

    [Fact]
    public void WeekendBackground_PaintsOneBoxPerWeekendDayBehindTheDays()
    {
        var calendar = new TestCalendar { ShownDate = May15, FirstDayOfWeek = DayOfWeek.Monday };

        calendar.WeekendDayBackgroundColor = Colors.LightGray;
        calendar.WeekendDayBackgroundCornerRadius = 8;

        var boxes = WeekendBoxes(calendar);
        boxes.Should().HaveCount(12, "two weekend columns in six weeks");
        boxes.Select(Grid.GetColumn).Distinct().Should().BeEquivalentTo([5, 6]);
        boxes.Select(Grid.GetRow).Distinct().Should().BeEquivalentTo([1, 2, 3, 4, 5, 6], "the title row is not covered");
        boxes.Should().AllSatisfy(box =>
        {
            box.BackgroundColor.Should().Be(Colors.LightGray);
            box.StrokeShape.Should().BeOfType<RoundRectangle>().Which.CornerRadius.Should().Be(new Microsoft.Maui.CornerRadius(8));
        });
        Days(calendar).Children.Take(12).Should().Equal(boxes, "the boxes are drawn behind the titles and the days");
    }

    [Fact]
    public void WeekendBackground_FollowsTheLayoutAndIsRemovedWhenTransparentAgain()
    {
        var calendar = new TestCalendar { ShownDate = May15, WeekendDayBackgroundColor = Colors.LightGray };

        calendar.CalendarLayout = WeekLayout.Week;
        WeekendBoxes(calendar).Should().HaveCount(2);
        WeekendBoxes(calendar).Select(Grid.GetColumn).Should().BeEquivalentTo([0, 6], "Sunday is first");

        calendar.WeekendDayBackgroundColor = Colors.Transparent;
        WeekendBoxes(calendar).Should().BeEmpty();

        calendar.WeekendDayBackgroundColor = Colors.LightGray;
        calendar.WeekendDayBackgroundColor = null!;
        WeekendBoxes(calendar).Should().BeEmpty();
    }

    [Fact]
    public void WeekendBackground_WithoutWeekRows_AddsNothing()
    {
        var calendar = new TestCalendar { ShownDate = May15 };
        var days = Days(calendar);
        days.Children.Clear();
        days.RowDefinitions.Clear();
        days.RowDefinitions.Add(new RowDefinition());

        calendar.WeekendDayBackgroundColor = Colors.LightGray;

        WeekendBoxes(calendar).Should().BeEmpty();
    }

    [Fact]
    public void WeekendDayColor_ColorsTheNumbersOfWeekendDays()
    {
        var calendar = new TestCalendar { ShownDate = May15 };

        calendar.WeekendDayColor = Colors.Red;

        calendar.DayFor(new DateTime(2025, 5, 17)).TextColor.Should().Be(Colors.Red);
        calendar.DayFor(new DateTime(2025, 5, 16)).TextColor.Should().NotBe(Colors.Red);
    }
}
