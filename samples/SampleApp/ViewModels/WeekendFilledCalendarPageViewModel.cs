using Plugin.Maui.Calendar.Models;

namespace SampleApp.ViewModels;

public partial class WeekendFilledCalendarPageViewModel : BasePageViewModel
{
    static readonly Color ShiftColor = Color.FromArgb("#2E9E4A");
    static readonly Color LeaveColor = Color.FromArgb("#E8941E");
    static readonly Color SickColor = Color.FromArgb("#C0392B");
    static readonly Color HolidayColor = Color.FromArgb("#E6B800");

    // Day-of-month -> (title, color), laid out to match the reference design.
    static readonly (int Day, string Title, Color Color)[] schedule =
    [
        (1, "Shift", ShiftColor),
        (3, "Shift", ShiftColor),
        (5, "Shift", ShiftColor),
        (8, "Shift", ShiftColor), (8, "Leave", LeaveColor),
        (10, "Shift", ShiftColor),
        (12, "Sick", SickColor),
        (17, "Shift", ShiftColor),
        (19, "Holiday", HolidayColor),
        (22, "Shift", ShiftColor),
        (23, "Shift", ShiftColor),
        (24, "Leave", LeaveColor),
        (25, "Leave", LeaveColor),
        (26, "Leave", LeaveColor),
        (29, "Shift", ShiftColor),
        (30, "Shift", ShiftColor), (30, "Sick", SickColor),
    ];

    public WeekendFilledCalendarPageViewModel() : base()
    {
        Events = BuildEvents();
    }

    public EventCollection Events { get; }

	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; } = DateTime.Today;

	static EventCollection BuildEvents()
    {
        var events = new EventCollection();
        var today = DateTime.Today;

        for (var offset = -1; offset <= 1; offset++)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(offset);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);

            var grouped = schedule
                .Where(s => s.Day <= daysInMonth)
                .GroupBy(s => monthStart.AddDays(s.Day - 1));

            foreach (var day in grouped)
            {
                var dayEvents = new DayEventCollection<EventModel>
                {
                    Colors = day.Select(s => s.Color).ToList(),
                };

                dayEvents.AddRange(day.Select(s => new EventModel
                {
                    Name = s.Title,
                    Description = $"{s.Title} — {day.Key:dd MMM}",
                    Color = s.Color,
                }));

                events[day.Key] = dayEvents;
            }
        }

        return events;
    }
}
