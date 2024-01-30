using Mopups.Pages;

namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CalendarRangePickerPopupSelectedDates : PopupPage
{
	readonly Action<CalendarRangePickerResult> onClosedPopup;

	public CalendarRangePickerPopupSelectedDates(Action<CalendarRangePickerResult> onClosedPopup)
	{
		this.onClosedPopup = onClosedPopup;
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is CalendarRangePickerPopupSelectedDatesViewModel vm)
		{
			vm.Closed += onClosedPopup;
		}
	}

	protected override void OnDisappearing()
	{
		if (BindingContext is CalendarRangePickerPopupSelectedDatesViewModel vm)
		{
			vm.Closed -= onClosedPopup;
		}

		base.OnDisappearing();
	}
}