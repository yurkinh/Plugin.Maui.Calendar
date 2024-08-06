namespace SampleApp.ViewModels;

public partial class Windows11CalendarViewModel : BasePageViewModel
{
    public Windows11CalendarViewModel()
    {

    }
    [ObservableProperty]
    int month = DateTime.Today.Month;

    [ObservableProperty]
    int year = DateTime.Today.Year;

    [ObservableProperty]
    DateTime? selectedDate = DateTime.Today;
}
