using System.Globalization;
using FluentAssertions;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Tests.TestHelpers;
using Xunit;
using CalendarControl = Plugin.Maui.Calendar.Controls.Calendar;

namespace Plugin.Maui.Calendar.Tests.Controls.Calendar;

/// <summary>
/// Verifies the texts the calendar writes with its <c>Culture</c>: the month name or week number in
/// the header, the year, the selected dates and the day numbers, with and without native digits.
/// </summary>
[Collection(MauiControlsCollection.Name)]
public class CultureTests
{
    static readonly DateTime October1 = new(2026, 10, 1);

    // ── Month names and dates of cultures whose own calendar is not Gregorian ─

    [Theory]
    [InlineData("fa-IR", "اکتبر")] // the default calendar is Persian (October is "مهر"/"دی" there)
    [InlineData("ar-SA", "أكتوبر")] // the default calendar is Um Al-Qura
    [InlineData("th-TH", "ตุลาคม")] // the default calendar is Thai Buddhist
    [InlineData("uk-UA", "Жовтень")]
    public void LayoutUnitText_IsTheGregorianMonthOfTheShownDays(string cultureName, string expectedMonth)
    {
        var calendar = new TestCalendar { ShownDate = October1, Culture = new CultureInfo(cultureName) };

        calendar.LayoutUnitText.Should().Be(expectedMonth);
    }

    [Theory]
    [InlineData("fa-IR", "1 اکتبر 2026")]
    [InlineData("th-TH", "1 ต.ค. 2026")] // not the Buddhist year 2569
    public void SelectedDateText_IsWrittenWithTheGregorianCalendar(string cultureName, string expected)
    {
        var calendar = new TestCalendar { ShownDate = October1, Culture = new CultureInfo(cultureName), SelectedDate = October1 };

        calendar.SelectedDateText.Should().Be(expected);
        calendar.LocalizedYear.Should().Be("2026");
    }

    [Fact]
    public void GetGregorianCulture_GregorianCulture_IsReturnedAsIs()
    {
        var culture = new CultureInfo("en-US");

        CalendarControl.GetGregorianCulture(culture).Should().BeSameAs(culture);
    }

    [Fact]
    public void GetGregorianCulture_OtherCalendar_ReturnsACopyWithTheLocalizedGregorianCalendar()
    {
        var culture = CultureInfo.GetCultureInfo("fa-IR");

        var gregorian = CalendarControl.GetGregorianCulture(culture);

        gregorian.Should().NotBeSameAs(culture);
        gregorian.Name.Should().Be("fa-IR");
        gregorian.DateTimeFormat.Calendar.Should().BeOfType<GregorianCalendar>()
            .Which.CalendarType.Should().Be(GregorianCalendarTypes.Localized);
        culture.DateTimeFormat.Calendar.Should().BeOfType<PersianCalendar>("the culture itself is not changed");
    }

    [Fact]
    public void Culture_ChangedAtRuntime_RewritesTheTexts()
    {
        var calendar = new TestCalendar { ShownDate = October1, SelectedDate = October1 };
        calendar.LayoutUnitText.Should().Be("October");

        calendar.Culture = new CultureInfo("fa-IR");
        calendar.LayoutUnitText.Should().Be("اکتبر");

        calendar.Culture = new CultureInfo("uk-UA");
        calendar.LayoutUnitText.Should().Be("Жовтень");
        calendar.SelectedDateText.Should().Be("1 жовт. 2026");
    }

    [Fact]
    public void SelectedDateTextFormat_ChangedAfterTheSelection_RewritesTheSelectedDateText()
    {
        var calendar = new TestCalendar { ShownDate = October1, SelectedDate = October1 };

        calendar.SelectedDateTextFormat = "dddd, d MMMM";

        calendar.SelectedDateText.Should().Be("Thursday, 1 October");
    }

    [Fact]
    public void Culture_SetToNull_FallsBackToTheInvariantCulture()
    {
        var calendar = new TestCalendar { ShownDate = October1, Culture = new CultureInfo("uk-UA") };

        calendar.Culture = null!;

        calendar.Culture.Should().BeSameAs(CultureInfo.InvariantCulture);
        calendar.LayoutUnitText.Should().Be("October");
    }

    // ── Week numbers ─────────────────────────────────────────────────────────

    [Fact]
    public void WeekNumber_IsCountedFromFirstDayOfWeek_SoEveryDayOfAShownWeekGivesTheSameNumber()
    {
        // With Monday as the first day, Monday Oct 5 to Sunday Oct 11 2026 is one row: ISO week 41.
        // The invariant culture starts weeks on Sunday, which used to give Sunday Oct 11 week 42.
        var calendar = new TestCalendar
        {
            CalendarLayout = WeekLayout.Week,
            WeekViewUnit = WeekViewUnit.WeekNumber,
            FirstDayOfWeek = DayOfWeek.Monday,
            ShownDate = new DateTime(2026, 10, 5),
        };

        calendar.LayoutUnitText.Should().Be("41");

        calendar.ShownDate = new DateTime(2026, 10, 11);

        calendar.LayoutUnitText.Should().Be("41");
    }

    [Fact]
    public void WeekNumber_UsesTheFirstFourDayWeekRule()
    {
        // Friday Jan 1 2027: the week of Mon Dec 28 2026 has only three days in 2027, so it is week 53.
        var calendar = new TestCalendar
        {
            WeekViewUnit = WeekViewUnit.WeekNumber,
            FirstDayOfWeek = DayOfWeek.Monday,
            ShownDate = new DateTime(2027, 1, 1),
        };

        calendar.LayoutUnitText.Should().Be("53");
    }

    [Fact]
    public void WeekViewUnit_ChangedAtRuntime_SwitchesBetweenMonthNameAndWeekNumber()
    {
        var calendar = new TestCalendar { FirstDayOfWeek = DayOfWeek.Monday, ShownDate = new DateTime(2026, 10, 5) };
        calendar.LayoutUnitText.Should().Be("October");

        calendar.WeekViewUnit = WeekViewUnit.WeekNumber;
        calendar.LayoutUnitText.Should().Be("41");

        calendar.WeekViewUnit = WeekViewUnit.MonthName;
        calendar.LayoutUnitText.Should().Be("October");
    }

    // ── Native digits ────────────────────────────────────────────────────────

    [Fact]
    public void UseNativeDigits_WritesDaysYearWeekNumberAndSelectedDateInTheCulturesDigits()
    {
        var calendar = new TestCalendar
        {
            Culture = new CultureInfo("ar-EG"),
            WeekViewUnit = WeekViewUnit.WeekNumber,
            FirstDayOfWeek = DayOfWeek.Monday,
            ShownDate = new DateTime(2026, 10, 5),
            // Spaces: the culture's "/" date separator carries right-to-left marks.
            SelectedDateTextFormat = "d M yyyy",
            SelectedDate = new DateTime(2026, 10, 12),
            UseNativeDigits = true,
        };

        calendar.DayFor(new DateTime(2026, 10, 12)).Day.Should().Be("١٢");
        calendar.LocalizedYear.Should().Be("٢٠٢٦");
        calendar.LayoutUnitText.Should().Be("٤١");
        calendar.SelectedDateText.Should().Be("١٢ ١٠ ٢٠٢٦");
    }

    [Fact]
    public void UseNativeDigits_ChangedAtRuntime_RewritesTheNumbersAndNotifiesLocalizedYear()
    {
        var calendar = new TestCalendar
        {
            Culture = new CultureInfo("ar-EG"),
            ShownDate = new DateTime(2026, 10, 5),
            SelectedDateTextFormat = "yyyy",
            SelectedDate = new DateTime(2026, 10, 12),
        };
        var changes = calendar.RecordPropertyChanges();
        calendar.DayFor(new DateTime(2026, 10, 12)).Day.Should().Be("12");

        calendar.UseNativeDigits = true;

        calendar.DayFor(new DateTime(2026, 10, 12)).Day.Should().Be("١٢");
        calendar.SelectedDateText.Should().Be("٢٠٢٦");
        changes.Should().Contain(nameof(CalendarControl.LocalizedYear));

        calendar.UseNativeDigits = false;

        calendar.DayFor(new DateTime(2026, 10, 12)).Day.Should().Be("12");
        calendar.LocalizedYear.Should().Be("2026");
    }

    [Fact]
    public void UseNativeDigits_CultureWithWesternDigits_KeepsWesternDigits()
    {
        var calendar = new TestCalendar { Culture = new CultureInfo("en-US"), ShownDate = October1, UseNativeDigits = true };

        calendar.DayFor(October1).Day.Should().Be("1");
        calendar.LocalizedYear.Should().Be("2026");
    }
}
