namespace SampleApp.Views;


public partial class SimplePage : ContentPage
{
    public SimplePage(SimplePageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }


}
