using System.Collections.ObjectModel;
using Mopups.Services;
using Plugin.Maui.Calendar.Enums;

namespace SampleApp.ViewModels;

public partial class CalendarRangePickerPopupSelectedDatesViewModel : BasePageViewModel
{
	public CalendarRangePickerPopupSelectedDatesViewModel()
	{
		SelectedDates =
		[
			DateTime.Today,
			DateTime.Today.AddDays(6),
		];
	}

	public event Action<CalendarRangePickerResult> Closed;

	[ObservableProperty]
	DateTime maximumDate = DateTime.Today.AddYears(1);

	[ObservableProperty]
	DateTime minimumDate = DateTime.Today.AddYears(-1);

	[ObservableProperty]
	DateTime shownDate = DateTime.Today;

	[ObservableProperty]
	ObservableCollection<DateTime> selectedDates = null;

	[ObservableProperty]
	WeekLayout calendarLayout = WeekLayout.Month;

	[RelayCommand]
	async Task Cancel()
	{
		Closed?.Invoke(new CalendarRangePickerResult() { IsSuccess = false });
		await MopupService.Instance.PopAsync();
	}

	[RelayCommand]
	void Clear()
	{
		SelectedDates = null;
	}

	[RelayCommand]
	async Task Success()
	{
		Closed?.Invoke(new CalendarRangePickerResult()
		{
			IsSuccess = true,
			SelectedDates = SelectedDates?.ToList()
		});
		await MopupService.Instance.PopAsync();
	}
}