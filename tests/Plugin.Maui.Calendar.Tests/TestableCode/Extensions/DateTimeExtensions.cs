using System.Globalization;

namespace Plugin.Maui.Calendar.Tests.TestableCode.Extensions;

public static class DateTimeExtensions
{
    public static string Capitalize(this string source)
    {
        if (source.Length == 0)
        {
            return source;
        }

        return char.ToUpperInvariant(source[0]) + source[1..];
    }

    public static DateTime StartDayOfMonth(this DateTime dt) => new DateTime(dt.Year, dt.Month, 1);

    public static DateTime EndDayOfMonth(this DateTime dt) => dt.StartDayOfMonth().AddMonths(1).AddDays(-1);

    public static int WeeksInMonth(this DateTime dateTime, CultureInfo culture)
    {
        var daysInMonth = DaysInMonth(dateTime);
        var date = new DateTime(dateTime.Year, dateTime.Month, daysInMonth);
        var lastWeekOfMonth = WeekOfMonth(date, culture);
        return lastWeekOfMonth;
    }

    public static int WeekOfMonth(this DateTime date, CultureInfo culture)
    {
        var weekOfYear = culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
        var weekOfYearForFirstDayOfMonth = culture.Calendar.GetWeekOfYear(date.FirstDayOfMonth(), CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
        var weekOfMonth = weekOfYear - weekOfYearForFirstDayOfMonth + 1;
        return weekOfMonth;
    }

    public static int DaysInMonth(this DateTime value)
    {
        return DateTime.DaysInMonth(value.Year, value.Month);
    }

    public static DateTime FirstDayOfMonth(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, 1);
    }
}