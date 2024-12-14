namespace SampleApp.ViewModels;

public partial class ToDoPageViewModel : BasePageViewModel
{
    public ToDoPageViewModel() : base()
    {

    }

    [ObservableProperty]
    int day = DateTime.Today.Day;

    [ObservableProperty]
    int month = DateTime.Today.Month;

    [ObservableProperty]
    int year = DateTime.Today.Year;
}