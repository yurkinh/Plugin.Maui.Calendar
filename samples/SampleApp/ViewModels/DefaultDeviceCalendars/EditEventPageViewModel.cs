using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SampleApp.ViewModels;

public partial class EditEventPageViewModel : BasePageViewModel, IQueryAttributable
{
	[ObservableProperty]
	AdvancedEventModel @event;

	[ObservableProperty]
	DateTime startingDate = DateTime.Now;

	[ObservableProperty]
	string name = string.Empty;

	[ObservableProperty]
	string description = string.Empty;

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("SelectedEvent", out var evt) && evt is AdvancedEventModel selectedEvent)
		{
			Event = selectedEvent;
			Name = selectedEvent.Name;
			Description = selectedEvent.Description;
			StartingDate = selectedEvent.Starting;
		}

		if (query.TryGetValue("SelectedDate", out var sDate) && sDate is DateTime selectedDate)
		{
			StartingDate = selectedDate;
		}
	}

	[RelayCommand]
	async Task AddEditEvent()
	{
		if (Event == null)
		{
			Event = new AdvancedEventModel
			{
				Name = Name,
				Description = Description,
				Starting = StartingDate
			};
		}
		else
		{
			Event.Name = Name;
			Event.Description = Description;
			Event.Starting = StartingDate;
		}

		await Shell.Current.GoToAsync("..", new Dictionary<string, object>
		{
			{ "Isbackwardnavigation", true },
			{ "UpdatedEvent", Event }
		});
	}

	[RelayCommand]
	static void Cancel()
	{
		Shell.Current.GoToAsync("..");
	}
}
