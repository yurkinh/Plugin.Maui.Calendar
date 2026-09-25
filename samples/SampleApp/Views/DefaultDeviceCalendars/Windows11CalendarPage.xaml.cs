namespace SampleApp.Views;


public partial class Windows11CalendarPage : ContentPage
{
    public Windows11CalendarPage(Windows11CalendarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}