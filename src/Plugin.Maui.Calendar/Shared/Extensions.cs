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
