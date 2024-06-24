using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using System.Collections.ObjectModel;

namespace SampleApp.ViewModels;

public partial class TwoWeekViewPageViewModel : BasePageViewModel
{
    public TwoWeekViewPageViewModel() : base()
    {
        // testing all kinds of adding events
        // when initializing collection
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-3)] = new List<EventModel>(GenerateEvents(10, "Cool")),
        };

        // with add method
        Events.Add(DateTime.Now.AddDays(-1), new List<EventModel>(GenerateEvents(5, "Cool")));

        // with indexer
        Events[DateTime.Now] = new List<EventModel>(GenerateEvents(2, "Boring"));
        // indexer - update later
        Events[DateTime.Now] = new ObservableCollection<EventModel>(GenerateEvents(10, "Cool"));

        // add later
        Events.Add(DateTime.Now.AddDays(3), new List<EventModel>(GenerateEvents(5, "Cool")));

        // indexer later
        Events[DateTime.Now.AddDays(10)] = new List<EventModel>(GenerateEvents(10, "Boring"));

        // add later
        Events.Add(DateTime.Now.AddDays(15), new List<EventModel>(GenerateEvents(10, "Cool")));

        // get observable collection later
        var todayEvents = Events[DateTime.Now] as ObservableCollection<EventModel>;

        // insert/add items to observable collection
        todayEvents.Insert(0, new EventModel { Name = "Cool event insert", Description = "This is Cool event's description!" });
        todayEvents.Add(new EventModel { Name = "Cool event add", Description = "This is Cool event's description!" });
    }
    public EventCollection Events { get; }

    [ObservableProperty]
    int day = DateTime.Today.Day;

    [ObservableProperty]
    int month = DateTime.Today.Month;

    [ObservableProperty]
    int year = DateTime.Today.Year;

    [ObservableProperty]
    DateTime shownDate = DateTime.Today;

    [ObservableProperty]
    WeekLayout calendarLayout = WeekLayout.TwoWeek;

    [ObservableProperty]
    DateTime? selectedDate = DateTime.Today;

    [ObservableProperty]
    DateTime minimumDate = DateTime.Today.AddYears(-2).AddMonths(-5);

    [ObservableProperty]
    DateTime maximumDate = DateTime.Today.AddMonths(5);

    private static IEnumerable<EventModel> GenerateEvents(int count, string name)
    {
        return Enumerable.Range(1, count).Select(x => new EventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!"
        });
    }
    [RelayCommand]
    static async Task EventSelected(object item)
    {
        if (item is EventModel eventModel)
        {
            await App.Current.MainPage.DisplayAlert(eventModel.Name, eventModel.Description, "Ok");
        }
    }

    [RelayCommand]
    void Today()
    {
        ShownDate = DateTime.Today;
        SelectedDate = DateTime.Today;
    }
}