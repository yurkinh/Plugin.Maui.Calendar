namespace SampleApp.Views;


public partial class TwoWeekViewPage : ContentPage
{
    public TwoWeekViewPage(TwoWeekViewPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
