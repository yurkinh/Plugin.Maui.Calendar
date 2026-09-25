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
	public partial DateTime MaximumDate { get; set; } = DateTime.Today.AddYears(1);

	[ObservableProperty]
	public partial DateTime MinimumDate { get; set; } = DateTime.Today.AddYears(-1);

	[ObservableProperty]
	public partial DateTime ShownDate { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial ObservableCollection<DateTime> SelectedDates { get; set; } = null;

	[ObservableProperty]
	public partial WeekLayout CalendarLayout { get; set; } = WeekLayout.Month;

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