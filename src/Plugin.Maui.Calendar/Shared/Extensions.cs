using System.Globalization;

namespace Plugin.Maui.Calendar;

static class Extensions
{
    internal static string Capitalize(this string source)
    {
        if (source.Length == 0)
		{
			return source;
		}

		return char.ToUpperInvariant(source[0]) + source[1..];
    }

    /// <summary>
    /// Returns the best display name for a weekday header given a character-count limit.
    /// When the culture's official abbreviated day name fits within <paramref name="maxLength"/>,
    /// it is preferred over plain character truncation of the full name — this avoids
    /// non-standard results for languages such as Russian where the first N characters of the
    /// full day name do not match the standard abbreviation (e.g. "пон" vs the correct "пн").
    /// </summary>
    internal static string TruncateDayName(this string fullName, string abbreviatedName, int maxLength)
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

    internal static object CreateContent(this DataTemplate dataTemplate, object itemModel)
    {
        if (dataTemplate is DataTemplateSelector templateSelector)
        {
            var template = templateSelector.SelectTemplate(itemModel, null);
            template.SetValue(BindableObject.BindingContextProperty, itemModel);

            return template.CreateContent();
        }

        dataTemplate.SetValue(BindableObject.BindingContextProperty, itemModel);
        return dataTemplate.CreateContent();
    }

    internal static DateTime StartDayOfMonth(this DateTime dt) => new DateTime(dt.Year, dt.Month, 1);

    internal static DateTime EndDayOfMonth(this DateTime dt) => dt.StartDayOfMonth().AddMonths(1).AddDays(-1);


    internal static int WeeksInMonth(this DateTime dateTime, CultureInfo culture)
    {
        var daysInMonth = DaysInMonth(dateTime);
        var date = new DateTime(dateTime.Year, dateTime.Month, daysInMonth);
        var lastWeekOfMonth = WeekOfMonth(date, culture);
        return lastWeekOfMonth;
    }

    internal static int WeekOfMonth(this DateTime date, CultureInfo culture)
    {
        var weekOfYear = culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
        var weekOfYearForFirstDayOfMonth = culture.Calendar.GetWeekOfYear(date.FirstDayOfMonth(), CalendarWeekRule.FirstDay, culture.DateTimeFormat.FirstDayOfWeek);
        var weekOfMonth = weekOfYear - weekOfYearForFirstDayOfMonth + 1;
        return weekOfMonth;
    }

    internal static int DaysInMonth(this DateTime value)
    {
        return DateTime.DaysInMonth(value.Year, value.Month);
    }

    internal static DateTime FirstDayOfMonth(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, 1);
    }
}
