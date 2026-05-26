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

    /// <summary>
    /// Returns the best display name for a weekday header given a character-count limit.
    /// When the official abbreviation fits within <paramref name="maxLength"/> it is preferred
    /// over plain character truncation of the full name.
    /// </summary>
    public static string TruncateDayName(this string fullName, string abbreviatedName, int maxLength)
    {
        // If the full name already fits within the requested limit, don't shorten it.
        if (fullName.Length <= maxLength)
        {
            return fullName;
        }

        return abbreviatedName.Length <= maxLength
            ? abbreviatedName
            : fullName[..Math.Min(maxLength, fullName.Length)];
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
        var weekOfYearForFirstDayOfMonth = culture.Calendar.GetWeekOfYear(date.StartDayOfMonth(), CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
        var weekOfMonth = weekOfYear - weekOfYearForFirstDayOfMonth + 1;
        return weekOfMonth;
    }

    public static int DaysInMonth(this DateTime value)
    {
        return DateTime.DaysInMonth(value.Year, value.Month);
    }

    [Obsolete("Use StartDayOfMonth instead.")]
    public static DateTime FirstDayOfMonth(this DateTime dt)
    {
        return dt.StartDayOfMonth();
    }
}