namespace SampleApp.Views;

public partial class XiaomiCalendarPage : ContentPage
{
    public XiaomiCalendarPage(XiaomiCalendarViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}