namespace SampleApp.Views;


public partial class SimplePage : ContentPage
{
    public SimplePage(SimplePageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }


    void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}
