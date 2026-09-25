using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;

namespace SampleApp.ViewModels;

public partial class AdvancedPageViewModel : BasePageViewModel
{
    public AdvancedPageViewModel() : base()
    {
        //uncoment if want to show alert when page is loaded
        //MainThread.BeginInvokeOnMainThread(async () => await Shell.Current.DisplayAlert("Info", "Loading events with delay, and changeing current view.", "Ok"));

        // testing all kinds of adding events
        // when initializing collection
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-3)] = new List<AdvancedEventModel>(GenerateEvents(10, "Cool")),
            [DateTime.Now.AddDays(-6)] = ColoredDay(
            [
                new() { Name = "Cool event1", Description = "This is Cool event1's description!", Starting= new DateTime() },
                new() { Name = "Cool event2", Description = "This is Cool event2's description!", Starting= new DateTime() }
            ], purple)
        };

        //Adding days with their own background color (EventIndicatorType is Background on this page)
        Events.Add(DateTime.Now.AddDays(-2), ColoredDay(GenerateEvents(10, "Cool"), blue));
        Events.Add(DateTime.Now.AddDays(-4), ColoredDay(GenerateEvents(10, "Cool"), green));
        Events.Add(DateTime.Now.AddDays(-5), ColoredDay(GenerateEvents(10, "Cool"), orange));

        // with add method
        Events.Add(DateTime.Now.AddDays(-1), new List<AdvancedEventModel>(GenerateEvents(5, "Cool")));

        // with indexer
        Events[DateTime.Now] = new List<AdvancedEventModel>(GenerateEvents(2, "Boring"));


        /*  Task.Delay(5000).ContinueWith(_ =>
         {
             // indexer - update later
             Events[DateTime.Now] = new ObservableCollection<AdvancedEventModel>(GenerateEvents(10, "Cool"));

             // add later
             Events.Add(DateTime.Now.AddDays(3), new List<AdvancedEventModel>(GenerateEvents(5, "Cool")));

             // indexer later
             Events[DateTime.Now.AddDays(10)] = new List<AdvancedEventModel>(GenerateEvents(10, "Boring"));

             // add later
             Events.Add(DateTime.Now.AddDays(15), new List<AdvancedEventModel>(GenerateEvents(10, "Cool")));


            Task.Delay(3000).ContinueWith(t =>
            {
                 // get observable collection later
                 var todayEvents = Events[DateTime.Now] as ObservableCollection<AdvancedEventModel>;

                 // insert/add items to observable collection
                 todayEvents.Insert(0, new AdvancedEventModel { Name = "Cool event insert", Description = "This is Cool event's description!", Starting = new DateTime() });
                 todayEvents.Add(new AdvancedEventModel { Name = "Cool event add", Description = "This is Cool event's description!", Starting = new DateTime() });
             }, TaskScheduler.FromCurrentSynchronizationContext());
         }, TaskScheduler.FromCurrentSynchronizationContext()); */

    }

    // Mid tones: white text stays readable on them in the light and the dark theme
    static readonly Color purple = Color.FromArgb("#7C5CBF");
    static readonly Color blue = Color.FromArgb("#3F7AD1");
    static readonly Color green = Color.FromArgb("#2E8B57");
    static readonly Color orange = Color.FromArgb("#C26A1B");

    static DayEventCollection<AdvancedEventModel> ColoredDay(IEnumerable<AdvancedEventModel> events, Color color) =>
        new(events)
        {
            EventIndicatorColor = color,
            EventIndicatorSelectedColor = color,
            EventIndicatorTextColor = Colors.White,
            EventIndicatorSelectedTextColor = Colors.White,
        };

    static IEnumerable<AdvancedEventModel> GenerateEvents(int count, string name)
    {
        return Enumerable.Range(1, count).Select(x => new AdvancedEventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!",
            Starting = new DateTime(2000, 1, 1, x * 2 % 24, x * 3 % 60, 0)
        });
    }

    public EventCollection Events { get; }

	[ObservableProperty]
	public partial DateTime ShownDate { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial WeekLayout CalendarLayout { get; set; } = WeekLayout.Month;
	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; } = DateTime.Today;

	[RelayCommand]
    static async Task DayTapped(DateTime date)
    {
        var message = $"Received tap event from date: {date}";

        await Shell.Current.DisplayAlertAsync("DayTapped", message, "Ok");

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
