using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SampleApp.ViewModels;

public partial class AdvancedPageViewModel : BasePageViewModel
{
    public AdvancedPageViewModel() : base()
    {
        MainThread.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Info", "Loading events with delay, and changeing current view.", "Ok"));

        Culture = CultureInfo.CreateSpecificCulture("en-GB");
        // testing all kinds of adding events
        // when initializing collection
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-3)] = new List<AdvancedEventModel>(GenerateEvents(10, "Cool")),
            [DateTime.Now.AddDays(-6)] = new DayEventCollection<AdvancedEventModel>(Colors.Purple, Colors.Purple)
            {
                new() { Name = "Cool event1", Description = "This is Cool event1's description!", Starting= new DateTime() },
                new() { Name = "Cool event2", Description = "This is Cool event2's description!", Starting= new DateTime() }
            }
        };

        //Adding a day with a different dot color
        Events.Add(DateTime.Now.AddDays(-2), new DayEventCollection<AdvancedEventModel>(GenerateEvents(10, "Cool")) { EventIndicatorColor = Colors.Blue, EventIndicatorSelectedColor = Colors.Blue });
        Events.Add(DateTime.Now.AddDays(-4), new DayEventCollection<AdvancedEventModel>(GenerateEvents(10, "Cool")) { EventIndicatorColor = Colors.Green, EventIndicatorSelectedColor = Colors.White });
        Events.Add(DateTime.Now.AddDays(-5), new DayEventCollection<AdvancedEventModel>(GenerateEvents(10, "Cool")) { EventIndicatorColor = Colors.Orange, EventIndicatorSelectedColor = Colors.Orange });

        // with add method
        Events.Add(DateTime.Now.AddDays(-1), new List<AdvancedEventModel>(GenerateEvents(5, "Cool")));

        // with indexer
        Events[DateTime.Now] = new List<AdvancedEventModel>(GenerateEvents(2, "Boring"));


        Task.Delay(5000).ContinueWith(_ =>
       {
           // indexer - update later
           Events[DateTime.Now] = new ObservableCollection<AdvancedEventModel>(GenerateEvents(10, "Cool"));

           // add later
           Events.Add(DateTime.Now.AddDays(3), new List<AdvancedEventModel>(GenerateEvents(5, "Cool")));

           // indexer later
           Events[DateTime.Now.AddDays(10)] = new List<AdvancedEventModel>(GenerateEvents(10, "Boring"));

           // add later
           Events.Add(DateTime.Now.AddDays(15), new List<AdvancedEventModel>(   GenerateEvents(10, "Cool")));

           Task.Delay(3000).ContinueWith(t =>
           {

               // get observable collection later
               var todayEvents = Events[DateTime.Now] as ObservableCollection<AdvancedEventModel>;

               // insert/add items to observable collection
               todayEvents.Insert(0, new AdvancedEventModel { Name = "Cool event insert", Description = "This is Cool event's description!", Starting = new DateTime() });
               todayEvents.Add(new AdvancedEventModel { Name = "Cool event add", Description = "This is Cool event's description!", Starting = new DateTime() });
           }, TaskScheduler.FromCurrentSynchronizationContext());
       }, TaskScheduler.FromCurrentSynchronizationContext());

    }

    private static IEnumerable<AdvancedEventModel> GenerateEvents(int count, string name)
    {
        return Enumerable.Range(1, count).Select(x => new AdvancedEventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!",
            Starting = new DateTime(2000, 1, 1, (x * 2) % 24, (x * 3) % 60, 0)
        });
    }

    public EventCollection Events { get; }

    [ObservableProperty]
    DateTime shownDate = DateTime.Today;

    [ObservableProperty]
    WeekLayout calendarLayout = WeekLayout.Month;

    [ObservableProperty]
    DateTime? selectedDate = DateTime.Today;

    [ObservableProperty]
    CultureInfo culture = CultureInfo.InvariantCulture;

    [RelayCommand]
    static async Task DayTapped(DateTime date)
    {
        var message = $"Received tap event from date: {date}";
        await App.Current.MainPage.DisplayAlert("DayTapped", message, "Ok");
    }

    [RelayCommand]
    static async Task EventSelected(object item)
    {
        if (item is AdvancedEventModel eventModel)
        {
            var title = $"Selected: {eventModel.Name}";
            var message = $"Starts: {eventModel.Starting:HH:mm}{Environment.NewLine}Details: {eventModel.Description}";
            await App.Current.MainPage.DisplayAlert(title, message, "Ok");
        }
    }
}
