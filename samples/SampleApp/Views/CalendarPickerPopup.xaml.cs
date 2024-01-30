using Mopups.Pages;

namespace SampleApp.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
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
}
