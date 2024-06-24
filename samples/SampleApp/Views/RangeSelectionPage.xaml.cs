namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class RangeSelectionPage : ContentPage
{
    public RangeSelectionPage()
    {
        InitializeComponent();
    }

    void UnloadedHandler(object sender, EventArgs e)
    {
        rangedCalendar.Dispose();
    }
}
