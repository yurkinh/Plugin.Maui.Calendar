using Mopups.Services;
using SampleApp.Views;

namespace SampleApp.ViewModels;

public class MainPageViewModel : BasePageViewModel
{
    public MainPageViewModel()
    {
        List<DemoGroup> groups =
        [
            new("Basics",
            [
                new("Default Calendar", "The control with no properties set", "\uf133", () => GoTo(nameof(CalendarPage))),
                new("Simple Event Calendar", "Events under the month, date limits and MonthChangedCommand", "\uf073", () => GoTo(nameof(SimplePage))),
                new("Advanced Event Calendar", "Custom header, footer and event templates, per-day dot colors", "\uf1de", () => GoTo(nameof(AdvancedPage))),
            ]),
            new("Selection",
            [
                new("Multi Selection", "Select any number of separate days", "\uf560", () => GoTo(nameof(MultiSelectionPage))),
                new("Range Selection", "Pick a start and an end date", "\uf07e", () => GoTo(nameof(RangeSelectionPage))),
                new("Week Selection", "One tap selects the whole week", "\uf25a", () => GoTo(nameof(WeekSelectionPage))),
            ]),
            new("Styling",
            [
                new("Weekend Calendar", "Weekend days and titles in their own color", "\uf5ca", () => GoTo(nameof(WeekendCalendarPage))),
                new("Weekend Filled Calendar", "Shaded weekend columns and multi-color event dots", "\uf0db", () => GoTo(nameof(WeekendFilledCalendarPage))),
                new("Day Template", "Every day cell drawn by your own DataTemplate", "\uf61f", () => GoTo(nameof(DayViewTemplatePage))),
            ]),
            new("Week View",
            [
                new("Single Week", "One week at a time, with a switch to the month", "\uf784", () => GoTo(nameof(WeekViewPage))),
                new("Two Weeks", "Two weeks at a time", "\uf5fd", () => GoTo(nameof(TwoWeekViewPage))),
            ]),
            new("Picker Popups",
            [
                new("Date Picker", "Choose one date in a popup", "\uf274", OpenDatePickerAsync),
                new("Range Picker: Selected Dates", "Returns every date of the chosen range", "\uf0ae", OpenRangePickerAsync),
                new("Range Picker: Start and End", "Returns the start and the end date", "\uf362", OpenStartEndRangePickerAsync),
            ]),
            new("Device Look-alikes",
            [
                new("Windows 11 Calendar", "The calendar flyout of the Windows 11 taskbar", "\uf2d0", () => GoTo(nameof(Windows11CalendarPage))),
            ]),
        ];

#if DEBUG
        groups.Add(new("Developer",
        [
            new("Testing Page", "Culture, day title length and abbreviated day names", "\uf0c3", () => GoTo(nameof(TestingPage))),
            new("Xiaomi Calendar", "Work in progress", "\uf3cf", () => GoTo(nameof(XiaomiCalendarPage))),
        ]));
#endif

        Groups = groups;
    }

    public IReadOnlyList<DemoGroup> Groups { get; }

    static Task GoTo(string route) => Shell.Current.GoToAsync(route);

    static Task OpenDatePickerAsync() =>
        MopupService.Instance.PushAsync(new CalendarPickerPopup(async result =>
        {
            var message = result.IsSuccess
                ? $"Received date from popup: {result.SelectedDate:dd/MM/yy}"
                : "Calendar Picker Canceled!";

            await Shell.Current.DisplayAlertAsync("Popup result", message, "Ok");
        }));

    static Task OpenRangePickerAsync() =>
        MopupService.Instance.PushAsync(new CalendarRangePickerPopupSelectedDates(async result =>
        {
            var message = "Calendar Range Picker Canceled!";

            if (result.IsSuccess && result.SelectedDates?.Count > 0)
            {
                message = $"Received date range from popup: {result.SelectedDates.Min():dd.MM.yyyy} - {result.SelectedDates.Max():dd.MM.yyyy}";
            }
            else if (result.IsSuccess)
            {
                message = "Nothing is selected!";
            }

            await Shell.Current.DisplayAlertAsync("Popup result", message, "Ok");
        }));

    static Task OpenStartEndRangePickerAsync() =>
        MopupService.Instance.PushAsync(new CalendarRangePickerPopup(async result =>
        {
            var message = "Calendar Range Picker Canceled!";

            if (result.IsSuccess && result.SelectedStartDate.HasValue && result.SelectedEndDate.HasValue)
            {
                message = $"Received date range from popup: {result.SelectedStartDate:dd.MM.yyyy} - {result.SelectedEndDate:dd.MM.yyyy}";
            }
            else if (result.IsSuccess)
            {
                message = "Nothing is selected!";
            }

            await Shell.Current.DisplayAlertAsync("Popup result", message, "Ok");
        }));
}
