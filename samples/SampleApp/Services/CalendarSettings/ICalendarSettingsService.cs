using System.Globalization;

namespace SampleApp.Services;

/// <summary>
/// Calendar options picked on the Settings tab and shared by every sample calendar.
/// </summary>
public interface ICalendarSettingsService
{
    /// <summary>
    /// Resource key of the chosen culture; SettingsCalendarStyle reads it as a DynamicResource.
    /// </summary>
    const string CultureResourceKey = "CalendarCulture";

    /// <summary>
    /// Resource key of the chosen first day of the week; SettingsCalendarStyle reads it as a DynamicResource.
    /// </summary>
    const string FirstDayOfWeekResourceKey = "CalendarFirstDayOfWeek";

    IReadOnlyList<CultureInfo> AvailableCultures { get; }
    IReadOnlyList<DayOfWeek> AvailableFirstDaysOfWeek { get; }
    CultureInfo Culture { get; set; }
    DayOfWeek FirstDayOfWeek { get; set; }

    /// <summary>
    /// Publishes the saved choices to the app resources. Call once the Application exists.
    /// </summary>
    void Apply();
}
