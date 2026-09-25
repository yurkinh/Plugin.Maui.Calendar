namespace SampleApp.Views;


public partial class RangeSelectionPage : ContentPage
{
    public RangeSelectionPage(RangeSelectionPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
