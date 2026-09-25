using Plugin.Maui.Calendar.Models;

namespace SampleApp.ViewModels;

public partial class NeoCalendarPageViewModel : BasePageViewModel
{

    public NeoCalendarPageViewModel() : base()
    {

        // testing all kinds of adding events
        // when initializing collection
        var threeEventsTommorrow = GenerateEvents(3, "Simple3").ToArray();
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-3)] = new List<EventModel>(GenerateEvents(10, "Cool")),
            [DateTime.Now.AddDays(4)] = new List<EventModel>(GenerateEvents(2, "Simple2")),
            [DateTime.Now.AddDays(2)] = new List<EventModel>(GenerateEvents(1, "Simple1")),
            [DateTime.Now.AddDays(1)] = new DayEventCollection<EventModel>(threeEventsTommorrow) { Colors = [.. threeEventsTommorrow.Select(e => e.Color)] },
        };

        // with add method
        Events.Add(DateTime.Now.AddDays(-1), new List<EventModel>(GenerateEvents(5, "Cool")));

        // with indexer
        Events[DateTime.Now] = new List<EventModel>(GenerateEvents(2, "Boring"));
    }

    public EventCollection Events { get; }

	[ObservableProperty]
	public partial int Day { get; set; } = DateTime.Today.Day;

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
	[ObservableProperty]
	public partial string Name { get; set; }

	[ObservableProperty]
	public partial string Description { get; set; }

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

    [RelayCommand]
    static async Task MonthChanged(MonthChangedEventArgs args)
    {
        string oldMonthName = new DateTime(1, args.OldMonth.Month, 1).ToString("MMMM");
        string newMonthName = new DateTime(1, args.NewMonth.Month, 1).ToString("MMMM");

        string message = $"From {oldMonthName} to {newMonthName}";
        await Shell.Current.DisplayAlertAsync("Month Changed", message, "OK");
    }

    static IEnumerable<EventModel> GenerateEvents(int count, string name)
    {
        return Enumerable.Range(1, count).Select(x => new EventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!",
            Color = Color.FromInt((int)(0xff000000 | Random.Shared.Next(0xffffff))),
        });
    }
}