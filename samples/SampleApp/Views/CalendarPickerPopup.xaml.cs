using Mopups.Pages;
namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CalendarPickerPopup : PopupPage
{
    private readonly Action<CalendarPickerResult> _onClosedPopup;

    public CalendarPickerPopup(Action<CalendarPickerResult> onClosedPopup)
    {
        _onClosedPopup = onClosedPopup;
        InitializeComponent();            
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CalendarPickerPopupViewModel vm)
            vm.Closed += _onClosedPopup;
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is CalendarPickerPopupViewModel vm)
            vm.Closed -= _onClosedPopup;

        base.OnDisappearing();
    }

    void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}
