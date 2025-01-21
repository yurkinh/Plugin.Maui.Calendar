namespace SampleApp.Views;


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
