using Plugin.Maui.Calendar.Models;

namespace SampleApp.ViewModels;

public partial class DayViewTemplatePageViewModel : BasePageViewModel
{
	public DayViewTemplatePageViewModel() : base()
	{
		Events = new EventCollection
		{
			[DateTime.Today] = EventsFor(
				Create("Design sync", "Review the new calendar look with the team", "#2DD4BF"),
				Create("Lunch", "Tacos with the mobile crew", "#84CC16")),
			[DateTime.Today.AddDays(-2)] = EventsFor(
				Create("Workshop", "MAUI layouts deep dive", "#22C3A6"),
				Create("1:1", "Weekly catch-up", "#14B8A6")),
			[DateTime.Today.AddDays(1)] = EventsFor(
				Create("Standup", "Daily sync at 10:00", "#0EA5E9")),
			[DateTime.Today.AddDays(3)] = EventsFor(
				Create("Release", "Ship v3.1 to NuGet", "#14B8A6"),
				Create("Changelog", "Publish release notes", "#16A34A"),
				Create("Party", "Celebrate the launch 🎉", "#84CC16")),
			[DateTime.Today.AddDays(8)] = EventsFor(
				Create("Review", "Sprint review & demo", "#22C3A6"),
				Create("Retro", "What went well, what to improve", "#0EA5E9")),
			// A busy day: the Tiles template shows its EventCount badge, the Photo template one dot per event.
			[DateTime.Today.AddDays(12)] = EventsFor(
				Create("Planning", "Pick the next sprint's stories", "#16A34A"),
				Create("Interview", "Mobile developer candidate", "#0EA5E9"),
				Create("Docs", "Write the DayViewTemplate guide", "#22C3A6"),
				Create("Meetup", ".NET MAUI community evening", "#84CC16")),
		};
	}

	static DayEventCollection<EventModel> EventsFor(params EventModel[] events) => new(events)
	{
		Colors = [.. events.Select(e => e.Color)],
	};

	static EventModel Create(string name, string description, string color) => new()
	{
		Name = name,
		Description = description,
		Color = Color.FromArgb(color),
	};

	public EventCollection Events { get; }

	public List<DateTime> DisabledDates { get; } =
	[
		DateTime.Today.AddDays(5),
		DateTime.Today.AddDays(6),
	];

	[ObservableProperty]
	public partial int Day { get; set; } = DateTime.Today.Day;

	[ObservableProperty]
	public partial int Month { get; set; } = DateTime.Today.Month;

	[ObservableProperty]
	public partial int Year { get; set; } = DateTime.Today.Year;

	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; } = DateTime.Today;

	[RelayCommand]
	void Today()
	{
		Year = DateTime.Today.Year;
		Month = DateTime.Today.Month;
		SelectedDate = DateTime.Today;
	}
}
