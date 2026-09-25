namespace SampleApp.ViewModels;

public partial class Windows11CalendarViewModel : BasePageViewModel
{
    public Windows11CalendarViewModel()
    {

    }
	[ObservableProperty]
	public partial int Month { get; set; } = DateTime.Today.Month;

	[ObservableProperty]
	public partial int Year { get; set; } = DateTime.Today.Year;
	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; } = DateTime.Today;
}
