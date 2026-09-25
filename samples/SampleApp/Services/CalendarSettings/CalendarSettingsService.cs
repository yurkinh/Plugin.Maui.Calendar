using System.Globalization;

namespace SampleApp.Services;

public class CalendarSettingsService : ICalendarSettingsService
{
    const string cultureKey = "calendar_culture";
    const string firstDayOfWeekKey = "calendar_first_day_of_week";

    CultureInfo culture;
    DayOfWeek firstDayOfWeek;

    public CalendarSettingsService()
    {
        var savedCulture = Preferences.Default.Get(cultureKey, "en-US");
        culture = AvailableCultures.FirstOrDefault(c => c.Name == savedCulture) ?? AvailableCultures[0];
        firstDayOfWeek = (DayOfWeek)Preferences.Default.Get(firstDayOfWeekKey, (int)DayOfWeek.Sunday);
    }

    public IReadOnlyList<CultureInfo> AvailableCultures { get; } =
    [
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("en-GB"),
        CultureInfo.GetCultureInfo("uk-UA"),
        CultureInfo.GetCultureInfo("pl-PL"),
        CultureInfo.GetCultureInfo("de-DE"),
        CultureInfo.GetCultureInfo("fr-FR"),
        CultureInfo.GetCultureInfo("es-ES"),
        CultureInfo.GetCultureInfo("it-IT"),
        CultureInfo.GetCultureInfo("ar-JO"),
        CultureInfo.GetCultureInfo("ja-JP"),
        CultureInfo.GetCultureInfo("zh-CN"),
    ];

    public IReadOnlyList<DayOfWeek> AvailableFirstDaysOfWeek { get; } =
    [
        DayOfWeek.Sunday,
        DayOfWeek.Monday,
        DayOfWeek.Saturday,
    ];

    public CultureInfo Culture
    {
        get => culture;
        set
        {
            culture = value;
            Preferences.Default.Set(cultureKey, value.Name);
            Apply();
        }
    }

    public DayOfWeek FirstDayOfWeek
    {
        get => firstDayOfWeek;
        set
        {
            firstDayOfWeek = value;
            Preferences.Default.Set(firstDayOfWeekKey, (int)value);
            Apply();
        }
    }

    public void Apply()
    {
        var resources = Application.Current!.Resources;
        resources[ICalendarSettingsService.CultureResourceKey] = culture;
        resources[ICalendarSettingsService.FirstDayOfWeekResourceKey] = firstDayOfWeek;
    }
}
