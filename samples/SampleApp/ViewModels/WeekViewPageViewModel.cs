using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using System.Collections.ObjectModel;

namespace SampleApp.ViewModels;

public partial class WeekViewPageViewModel : BasePageViewModel
{
    public WeekViewPageViewModel() : base()
    {
        // testing all kinds of adding events
        // when initializing collection
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-3)] = new List<EventModel>(WeekViewPageViewModel.GenerateEvents(10, "Cool")),
        };

        // with add method
        Events.Add(DateTime.Now.AddDays(-1), new List<EventModel>(WeekViewPageViewModel.GenerateEvents(5, "Cool")));

        // with indexer
        Events[DateTime.Now] = new List<EventModel>(WeekViewPageViewModel.GenerateEvents(2, "Boring"));
        // indexer - update later
        Events[DateTime.Now] = new ObservableCollection<EventModel>(WeekViewPageViewModel.GenerateEvents(10, "Cool"));

        // add later
        Events.Add(DateTime.Now.AddDays(3), new List<EventModel>(WeekViewPageViewModel.GenerateEvents(5, "Cool")));

        // indexer later
        Events[DateTime.Now.AddDays(10)] = new List<EventModel>(WeekViewPageViewModel.GenerateEvents(10, "Boring"));

        // add later
        Events.Add(DateTime.Now.AddDays(15), new List<EventModel>(WeekViewPageViewModel.GenerateEvents(10, "Cool")));

        // get observable collection later
        var todayEvents = Events[DateTime.Now] as ObservableCollection<EventModel>;

        // insert/add items to observable collection
        todayEvents.Insert(0, new EventModel { Name = "Cool event insert", Description = "This is Cool event's description!" });
        todayEvents.Add(new EventModel { Name = "Cool event add", Description = "This is Cool event's description!" });
    }
    public EventCollection Events { get; }

	[ObservableProperty]
	public partial int Day { get; set; } = DateTime.Today.Day;

	[ObservableProperty]
	public partial int Month { get; set; } = DateTime.Today.Month;
	[ObservableProperty]
	public partial int Year { get; set; } = DateTime.Today.Year;

	[ObservableProperty]
	public partial DateTime ShownDate { get; set; } = DateTime.Today;
	[ObservableProperty]
	public partial WeekLayout CalendarLayout { get; set; } = WeekLayout.Week;

	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; } = DateTime.Today;
	[ObservableProperty]
	public partial DateTime MinimumDate { get; set; } = DateTime.Today.AddYears(-2).AddMonths(-5);

	[ObservableProperty]
	public partial DateTime MaximumDate { get; set; } = DateTime.Today.AddMonths(5);
	[ObservableProperty]
	public partial string Name { get; set; }

	[ObservableProperty]
	public partial string Description { get; set; }

	static IEnumerable<EventModel> GenerateEvents(int count, string name)
    {
        return Enumerable.Range(1, count).Select(x => new EventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!"
        });
    }

    [RelayCommand]
    void Today()
    {
        ShownDate = DateTime.Today;
        SelectedDate = DateTime.Today;
    }

    [RelayCommand]
    static async Task EventSelected(object item)
    {
        if (item is EventModel eventModel)
        {
            await Shell.Current.DisplayAlertAsync(eventModel.Name, eventModel.Description, "Ok");
        }
    }
}