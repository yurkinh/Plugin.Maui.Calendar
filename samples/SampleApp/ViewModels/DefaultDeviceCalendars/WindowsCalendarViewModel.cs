namespace SampleApp.ViewModels;

public partial class WindowsCalendarViewModel : BasePageViewModel
{
	public WindowsCalendarViewModel()
	{

	}
	[ObservableProperty]
	int month = DateTime.Today.Month;

	[ObservableProperty]
	int year = DateTime.Today.Year;

	[ObservableProperty]
	DateTime? selectedDate = DateTime.Today;
}
