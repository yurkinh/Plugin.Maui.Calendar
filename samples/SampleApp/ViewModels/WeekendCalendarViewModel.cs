using Plugin.Maui.Calendar.Models;
using SampleApp.Model;
namespace SampleApp.ViewModels;
public partial class WeekendCalendarPageViewModel : BasePageViewModel
{
    public WeekendCalendarPageViewModel() : base()
    {
        MainThread.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Info", "Loading events with delay, and changeing current view.", "Ok"));

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

        // with indexer
        Events[DateTime.Now] = new List<EventModel>(GenerateEvents(2, "Boring"));
    }


    static IEnumerable<EventModel> GenerateEvents(int count, string name)
    {
        return Enumerable.Range(1, count).Select(x => new EventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!"
        });
    }

    public EventCollection Events { get; }

    private int _month = DateTime.Today.Month;

    public int Month
    {
        get => _month;
        set => SetProperty(ref _month, value);
    }

    private int _year = DateTime.Today.Year;

    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    private DateTime? _selectedDate = DateTime.Today;

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    private DateTime _minimumDate = new DateTime(2019, 4, 29);

    public DateTime MinimumDate
    {
        get => _minimumDate;
        set => SetProperty(ref _minimumDate, value);
    }

    private DateTime _maximumDate = DateTime.Today.AddMonths(5);

    public DateTime MaximumDate
    {
        get => _maximumDate;
        set => SetProperty(ref _maximumDate, value);
    }


    private async Task ExecuteEventSelectedCommand(object item)
    {
        if (item is EventModel eventModel)
        {
            await App.Current.MainPage.DisplayAlert(eventModel.Name, eventModel.Description, "Ok");
        }
    }
}
