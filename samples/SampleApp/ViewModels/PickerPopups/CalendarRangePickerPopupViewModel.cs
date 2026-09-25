using Mopups.Services;
using Plugin.Maui.Calendar.Enums;

namespace SampleApp.ViewModels;

public partial class CalendarRangePickerPopupViewModel : BasePageViewModel
{

    public event Action<CalendarRangePickerResult> Closed;

	[ObservableProperty]
	public partial DateTime MaximumDate { get; set; } = DateTime.Today.AddYears(1);

	[ObservableProperty]
	public partial DateTime MinimumDate { get; set; } = DateTime.Today.AddYears(-1);
	[ObservableProperty]
	public partial DateTime ShownDate { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial WeekLayout CalendarLayout { get; set; } = WeekLayout.Month;
	[ObservableProperty]
	public partial DateTime? SelectedStartDate { get; set; } = DateTime.Today.AddDays(-5);

	[ObservableProperty]
	public partial DateTime? SelectedEndDate { get; set; } = DateTime.Today.AddDays(5);

	[RelayCommand]
    async Task Cancel()
    {
        Closed?.Invoke(new CalendarRangePickerResult() { IsSuccess = false });
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    void Clear()
    {
        SelectedEndDate = null;
        SelectedStartDate = null;
    }

    [RelayCommand]
    async Task Success()
    {
        Closed?.Invoke(new CalendarRangePickerResult()
        {
            IsSuccess = true,
            SelectedStartDate = SelectedStartDate,
            SelectedEndDate = SelectedEndDate
        });
        await MopupService.Instance.PopAsync();
    }
}
