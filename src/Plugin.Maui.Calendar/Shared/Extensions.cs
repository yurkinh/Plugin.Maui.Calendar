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

    internal static string NormalizeDayName(this string source, CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName is "ar" && source.StartsWith("ال", StringComparison.Ordinal)
            ? source[2..]
            : source;
    }

    /// <summary>
    /// Returns the template to use for <paramref name="item"/>: the choice of a
    /// <see cref="DataTemplateSelector"/>, which receives <paramref name="container"/> (the view
    /// that will host the content), or <paramref name="dataTemplate"/> itself otherwise.
    /// Returns <see langword="null"/> when the selector chooses no template.
    /// </summary>
    internal static DataTemplate SelectDataTemplate(this DataTemplate dataTemplate, object item, BindableObject container)
    {
        return dataTemplate is DataTemplateSelector templateSelector
            ? templateSelector.SelectTemplate(item, container)
            : dataTemplate;
    }

    /// <summary>
    /// Creates the content of <paramref name="dataTemplate"/> for <paramref name="item"/> (resolving a
    /// <see cref="DataTemplateSelector"/> first), to be shown in <paramref name="container"/>.
    /// Returns <see langword="null"/> when the selector chooses no template.
    /// </summary>
    /// <remarks>
    /// The template is never modified. Templates are usually shared resources, and a value set on
    /// one (<see cref="DataTemplate.SetValue"/>) is applied to every view it creates afterwards, so
    /// setting the item there would leak it into other views and keep it alive. The created content
    /// gets its binding context by inheritance when it is added to <paramref name="container"/>,
    /// whose binding context is <paramref name="item"/>.
    /// </remarks>
    internal static object CreateContent(this DataTemplate dataTemplate, object item, BindableObject container)
    {
        return dataTemplate.SelectDataTemplate(item, container)?.CreateContent();
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
