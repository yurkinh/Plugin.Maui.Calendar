using Plugin.Maui.Calendar.Models;

namespace SampleApp.ViewModels;
public partial class WeekendCalendarPageViewModel : BasePageViewModel
{
    public WeekendCalendarPageViewModel() : base()
    {
        //uncoment if want to show alert when page is loaded
        //MainThread.BeginInvokeOnMainThread(async () => await AppShell.Current.DisplayAlert("Info", "Loading events with delay, and changeing current view.", "Ok"));

        // testing all kinds of adding events
        // when initializing collection
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-3)] = new List<EventModel>(GenerateEvents(10, "Cool")),
            [DateTime.Now.AddDays(4)] = new List<EventModel>(GenerateEvents(2, "Simple2")),
            [DateTime.Now.AddDays(2)] = new List<EventModel>(GenerateEvents(1, "Simple1")),
            [DateTime.Now.AddDays(1)] = new List<EventModel>(GenerateEvents(3, "Simple3")),
        };
        // with add method
        Events.Add(DateTime.Now.AddDays(-1), new List<EventModel>(GenerateEvents(5, "Cool")));
    }

    public EventCollection Events { get; }

	[ObservableProperty]
	public partial int Month { get; set; } = DateTime.Today.Month;

	[ObservableProperty]
	public partial int Year { get; set; } = DateTime.Today.Year;
	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial DateTime MinimumDate { get; set; } = new(2019, 4, 29);
	[ObservableProperty]
	public partial DateTime MaximumDate { get; set; } = DateTime.Today.AddMonths(5);

	static IEnumerable<EventModel> GenerateEvents(int count, string name)
    {
        return Enumerable.Range(1, count).Select(x => new EventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!"
        });
    }

    [RelayCommand]
    static async Task ExecuteEventSelected(object item)
    {
        if (item is EventModel eventModel)
        {
            await Shell.Current.DisplayAlertAsync(eventModel.Name, eventModel.Description, "Ok");
        }
    }

    [RelayCommand]
    void Today()
    {
        Year = DateTime.Today.Year;
        Month = DateTime.Today.Month;
    }

    [RelayCommand]
    static async Task EventSelected(object item)
    {
        if (item is AdvancedEventModel eventModel)
        {
            var title = $"Selected: {eventModel.Name}";
            var message = $"Starts: {eventModel.Starting:HH:mm}{Environment.NewLine}Details: {eventModel.Description}";
            await Shell.Current.DisplayAlertAsync(title, message, "Ok");
        }
    }
}