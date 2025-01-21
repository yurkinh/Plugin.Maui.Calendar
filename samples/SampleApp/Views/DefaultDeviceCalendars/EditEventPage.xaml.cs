namespace SampleApp.Views;

public partial class EditEventPage : ContentPage
{
    public EditEventPage(EditEventPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
