using Mopups.Pages;
namespace SampleApp.Views;


public partial class CalendarPickerPopup : PopupPage
{
    readonly Action<CalendarPickerResult> onClosedPopup;

    public CalendarPickerPopup(Action<CalendarPickerResult> onClosedPopup)
    {
		this.onClosedPopup = onClosedPopup;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CalendarPickerPopupViewModel vm)
        {
            vm.Closed += onClosedPopup;
        }
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is CalendarPickerPopupViewModel vm)
        {
            vm.Closed -= onClosedPopup;
        }

        base.OnDisappearing();
    }

    void UnloadedHandler(object sender, EventArgs e)
    {
        calendar.Dispose();
    }
}
