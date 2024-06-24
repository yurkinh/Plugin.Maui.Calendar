namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TwoWeekViewPage : ContentPage
{
    public TwoWeekViewPage()
    {
        InitializeComponent();
    }

    void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}
