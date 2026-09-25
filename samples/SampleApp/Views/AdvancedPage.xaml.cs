namespace SampleApp.Views;


public partial class AdvancedPage : ContentPage
{
    public AdvancedPage(AdvancedPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
