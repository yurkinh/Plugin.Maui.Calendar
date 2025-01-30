using Plugin.Maui.Calendar.Models;
using System.Collections.ObjectModel;

namespace SampleApp.ViewModels;

public partial class SimplePageViewModel : BasePageViewModel
{
    public SimplePageViewModel() : base()
    {

        // testing all kinds of adding events
        // when initializing collection
        var threeEventsTommorrow = GenerateEvents(3, "Simple3").ToArray();
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-3)] = new List<EventModel>(GenerateEvents(10, "Cool")),
            [DateTime.Now.AddDays(4)] = new List<EventModel>(GenerateEvents(2, "Simple2")),
            [DateTime.Now.AddDays(2)] = new List<EventModel>(GenerateEvents(1, "Simple1")),
            [DateTime.Now.AddDays(1)] = new DayEventCollection<EventModel>(threeEventsTommorrow) { Colors = threeEventsTommorrow.Select(e => e.Color).ToArray() },
        };

        // with add method
        Events.Add(DateTime.Now.AddDays(-1), new List<EventModel>(GenerateEvents(5, "Cool")));

        // with indexer
        Events[DateTime.Now] = new List<EventModel>(GenerateEvents(2, "Boring"));


        /* Task.Delay(5000).ContinueWith(_ =>
        {
            // indexer - update later
            Events[DateTime.Now] = new ObservableCollection<EventModel>(GenerateEvents(10, "Cool"));

            // add later
            Events.Add(DateTime.Now.AddDays(3), new List<EventModel>(GenerateEvents(5, "Cool")));

            // indexer later
            Events[DateTime.Now.AddDays(10)] = new List<EventModel>(GenerateEvents(10, "Boring"));

            // add later
            Events.Add(DateTime.Now.AddDays(15), new List<EventModel>(GenerateEvents(10, "Cool")));

            Month += 1;

            Task.Delay(3000).ContinueWith(t =>
            {
                // get observable collection later
                var todayEvents = Events[DateTime.Now] as ObservableCollection<EventModel>;

                // insert/add items to observable collection
                todayEvents.Insert(0, new EventModel { Name = "Cool event insert", Description = "This is Cool event's description!" });
                todayEvents.Add(new EventModel { Name = "Cool event add", Description = "This is Cool event's description!" });

                Month += 1;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }, TaskScheduler.FromCurrentSynchronizationContext()); */
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

    public EventCollection Events { get; }

    [ObservableProperty]
    int day = DateTime.Today.Day;

    [ObservableProperty]
    int month = DateTime.Today.Month;

    [ObservableProperty]
    int year = DateTime.Today.Year;

    [ObservableProperty]
    DateTime? selectedDate = DateTime.Today;

    [ObservableProperty]
    DateTime minimumDate = new DateTime(2019, 4, 29);

    [ObservableProperty]
    DateTime maximumDate = DateTime.Today.AddMonths(5);

    [ObservableProperty]
    string name;

    [ObservableProperty]
    string description;

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
            await Shell.Current.DisplayAlert(title, message, "Ok");
        }
    }

    [RelayCommand]
    async Task MonthChanged(MonthChangedEventArgs args)
    {
        string oldMonthName = new DateTime(1, args.OldMonth.Month, 1).ToString("MMMM");
        string newMonthName = new DateTime(1, args.NewMonth.Month, 1).ToString("MMMM");

        string message = $"From {oldMonthName} to {newMonthName}";
        await Shell.Current.DisplayAlert("Month Changed", message, "OK");
    }
}