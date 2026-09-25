using Plugin.Maui.Calendar.Models;
using SampleApp.Views;

namespace SampleApp.ViewModels;

public partial class XiaomiCalendarViewModel : BasePageViewModel, IQueryAttributable
{
    public XiaomiCalendarViewModel() : base()
    {
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(1)] = new DayEventCollection<AdvancedEventModel>(Colors.Purple, Colors.Green)
            {
                new() { Name = "Cool event1", Description = "This is Cool event1's description!", Starting= new DateTime() },
                new() { Name = "Cool event2", Description = "This is Cool event2's description!", Starting= new DateTime() }
            }
        };
    }

    public EventCollection Events { get; set; }

	[ObservableProperty]
	public partial int Day { get; set; } = DateTime.Today.Day;

	[ObservableProperty]
	public partial int Month { get; set; } = DateTime.Today.Month;
	[ObservableProperty]
	public partial int Year { get; set; } = DateTime.Today.Year;

	[ObservableProperty]
	public partial DateTime SelectedDate { get; set; } = DateTime.Today;

	public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Isbackwardnavigation", out var isBackwardNavigation) && (bool)isBackwardNavigation)
        {
            if (query.TryGetValue("UpdatedEvent", out var evt) && evt is AdvancedEventModel updatedEvent)
            {
                if (Events.TryGetValue(updatedEvent.Starting, out var existingCollection))
                {
                    var newEventCollection = new DayEventCollection<AdvancedEventModel>(Colors.Purple, Colors.Green);

                    foreach (var item in existingCollection)
                    {
                        if (item is AdvancedEventModel eventModel && eventModel.Name != updatedEvent.Name)
                        {
                            newEventCollection.Add(eventModel);
                        }
                    }

                    newEventCollection.Add(updatedEvent); 
                    Events[updatedEvent.Starting] = newEventCollection;
                }
                else
                {
                    Events[updatedEvent.Starting] = new DayEventCollection<AdvancedEventModel>(Colors.Purple, Colors.Green) { updatedEvent };
                }
            }
        }
    }


    [RelayCommand]
    public async Task EventTapped(AdvancedEventModel tappedEvent)
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedEvent", tappedEvent },
            { "SelectedDate",  SelectedDate}
        };

        await Shell.Current.GoToAsync(nameof(EditEventPage), navigationParameter);
	}

    [RelayCommand]
    public async Task AddEvent()
    {
        var navigationParameter = new Dictionary<string, object>
        {
            { "SelectedDate",  SelectedDate}
        };
        await Shell.Current.GoToAsync(nameof(EditEventPage), navigationParameter);
    }
}
