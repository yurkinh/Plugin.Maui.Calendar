using System.Collections.ObjectModel;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;

namespace SampleApp.ViewModels;

public partial class RangeSelectionPageViewModel : BasePageViewModel
{
    public RangeSelectionPageViewModel() : base()
    {
        // testing all kinds of adding events
        // when initializing collection
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(-1)] = new List<AdvancedEventModel>(GenerateEvents(5, "Cool", DateTime.Now.AddDays(-1))),
            [DateTime.Now.AddDays(-2)] = new DayEventCollection<AdvancedEventModel>(GenerateEvents(10, "Cool", DateTime.Now.AddDays(-2))),
            [DateTime.Now.AddDays(-4)] = new DayEventCollection<AdvancedEventModel>(GenerateEvents(10, "Super Cool", DateTime.Now.AddDays(-4))),
            [DateTime.Now.AddDays(-5)] = new DayEventCollection<AdvancedEventModel>(GenerateEvents(10, "Cool", DateTime.Now.AddDays(-5))),
            [DateTime.Now.AddDays(-6)] = new DayEventCollection<AdvancedEventModel>(Colors.Purple, Colors.Purple)
            {
                new() { Name = "Cool event1", Description = "This is Cool event1's description!", Starting= DateTime.Now.AddDays(-6)},
                new() { Name = "Cool event2", Description = "This is Cool event2's description!", Starting= DateTime.Now.AddDays(-6)}
            },
            [DateTime.Now.AddDays(-10)] = new List<AdvancedEventModel>(GenerateEvents(10, "Cool", DateTime.Now.AddDays(-10))),
            [DateTime.Now.AddDays(1)] = new List<AdvancedEventModel>(GenerateEvents(2, "Boring", DateTime.Now.AddDays(1))),
            [DateTime.Now.AddDays(4)] = new List<AdvancedEventModel>(GenerateEvents(10, "Cool", DateTime.Now.AddDays(4))),
            [DateTime.Now.AddDays(8)] = new List<AdvancedEventModel>(GenerateEvents(1, "Cool", DateTime.Now.AddDays(8))),
            [DateTime.Now.AddDays(9)] = new List<AdvancedEventModel>(GenerateEvents(10, "Cool H", DateTime.Now.AddDays(9))),
            [DateTime.Now.AddDays(10)] = new List<AdvancedEventModel>(GenerateEvents(100, "Cool X", DateTime.Now.AddDays(10))),
            [DateTime.Now.AddDays(16)] = new List<AdvancedEventModel>(GenerateEvents(7, "Cool B", DateTime.Now.AddDays(16))),
            [DateTime.Now.AddDays(20)] = new List<AdvancedEventModel>(GenerateEvents(9, "Cool A", DateTime.Now.AddDays(20))),
            [DateTime.Now.AddDays(35)] = new List<AdvancedEventModel>(GenerateEvents(1, "Cool S", DateTime.Now.AddDays(35))),
            [DateTime.Now.AddDays(43)] = new List<AdvancedEventModel>(GenerateEvents(4, "Cool Q", DateTime.Now.AddDays(43))),
            [DateTime.Now.AddDays(46)] = new List<AdvancedEventModel>(GenerateEvents(12, "Cool ZZ", DateTime.Now.AddDays(46))),
            [DateTime.Now.AddDays(51)] = new List<AdvancedEventModel>(GenerateEvents(3, "Cool Y", DateTime.Now.AddDays(51))),
        };
    }

    [ObservableProperty]
    DateTime? selectedEndDate = DateTime.Today.AddDays(2);

    [ObservableProperty]
    DateTime shownDate = DateTime.Today;

    [ObservableProperty]
    WeekLayout calendarLayout = WeekLayout.Month;

    [ObservableProperty]
    ObservableCollection<DateTime> selectedDates = [];

    [ObservableProperty]
    DateTime? selectedStartDate = DateTime.Today.AddDays(-9);

    public EventCollection Events { get; }

    [RelayCommand]
    static async Task EventSelected(object item)
    {
        if (item is AdvancedEventModel eventModel)
        {
            var title = $"Selected: {eventModel.Name}";
            var message = $"Starts: {eventModel.Starting:HH:mm}{Environment.NewLine}Details: {eventModel.Description}";

			if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page is not null)
			{
				await Application.Current.Windows[0].Page.DisplayAlertAsync(title, message, "Ok");
			}
		}
	}

	[RelayCommand]
    static void DayTapped(object item)
    {

    }


    static IEnumerable<AdvancedEventModel> GenerateEvents(int count, string name, DateTime timeOfEvent)
    {
        return Enumerable.Range(1, count).Select(x => new AdvancedEventModel
        {
            Name = $"{name} event{x}",
            Description = $"This is {name} event{x}'s description!",
            Starting = new DateTime(timeOfEvent.Year, timeOfEvent.Month, timeOfEvent.Day, (x * 2) % 24, (x * 3) % 60, 0)
        });
    }
}