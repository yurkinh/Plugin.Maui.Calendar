using Mopups.Pages;
namespace SampleApp.Views;


public partial class CalendarRangePickerPopup : PopupPage
{
    readonly Action<CalendarRangePickerResult> onClosedPopup;

    public CalendarRangePickerPopup(Action<CalendarRangePickerResult> onClosedPopup)
    {
		this.onClosedPopup = onClosedPopup;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CalendarRangePickerPopupViewModel vm)
		{
			vm.Closed += onClosedPopup;
		}
	}

    protected override void OnDisappearing()
    {
        if (BindingContext is CalendarRangePickerPopupViewModel vm)
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
