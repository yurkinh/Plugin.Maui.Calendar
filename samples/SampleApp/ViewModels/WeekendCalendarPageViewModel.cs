using Plugin.Maui.Calendar.Models;

namespace SampleApp.ViewModels;

public partial class WeekendCalendarPageViewModel : BasePageViewModel
{
	public string SelectedThemeEntry { get; set; }

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

	[RelayCommand]
	static async Task ExecuteEventSelected(object item)
	{
		if (item is EventModel eventModel)
		{
			await App.Current.MainPage.DisplayAlert(eventModel.Name, eventModel.Description, "Ok");
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
			await App.Current.MainPage.DisplayAlert(title, message, "Ok");
		}
	}

	[ObservableProperty]
	bool isSystemChecked;
	partial void OnIsSystemCheckedChanged(bool value)
	{
		App.Current.MainPage.DisplayAlert("You have chosen a System theme", value.ToString(), "OK");
	}

	[ObservableProperty]
	bool isDarkChecked;
	partial void OnIsDarkCheckedChanged(bool value)
	{
		App.Current.MainPage.DisplayAlert("You have chosen a Dark theme", value.ToString(), "OK");
	}

	[ObservableProperty]
	bool isLightChecked;
	partial void OnIsLightCheckedChanged(bool value)
	{
		App.Current.MainPage.DisplayAlert("You have chosen a Light theme", value.ToString(), "OK");
	}


}
