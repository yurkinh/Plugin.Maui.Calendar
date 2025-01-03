using Plugin.Maui.Calendar.Models;
using Mopups.Services;
using SampleApp.Helpers;
using SampleApp.Views;

namespace SampleApp.ViewModels;

public partial class ToDoPageViewModel : BasePageViewModel
{
    public ToDoPageViewModel() : base()
    {
        Events = new EventCollection
        {
            [DateTime.Now.AddDays(3)] = new DayEventCollection<AdvancedEventModel>(Colors.Purple, Colors.Green)
            {
                new() { Name = "Cool event1", Description = "This is Cool event1's description!", Starting= new DateTime() },
                new() { Name = "Cool event2", Description = "This is Cool event2's description!", Starting= new DateTime() }
            }
        };
    }

    public EventCollection Events { get; }

    [ObservableProperty]
    int day = DateTime.Today.Day;

    [ObservableProperty]
    int month = DateTime.Today.Month;

    [ObservableProperty]
    int year = DateTime.Today.Year;

    [RelayCommand]
    public async Task AddEvent()
    {
        await Navigation.PushAsync(ServiceHelper.GetService<EditEventPage>());
    }
}
