using Mopups.Services;
using Plugin.Maui.Calendar.Enums;

namespace SampleApp.ViewModels;

public partial class CalendarPickerPopupViewModel : BasePageViewModel
{
    public CalendarPickerPopupViewModel() : base()
    {
        SelectedDate = new DateTime(2021, 6, 13);
    }

    public event Action<CalendarPickerResult> Closed;

	[ObservableProperty]
	public partial DateTime ShownDate { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial WeekLayout CalendarLayout { get; set; } = WeekLayout.Month;
	[ObservableProperty]
	public partial DateTime? SelectedDate { get; set; }

	[ObservableProperty]
	public partial DateTime MinimumDate { get; set; } = new(1900, 1, 1);
	[ObservableProperty]
	public partial DateTime MaximumDate { get; set; } = DateTime.Today.AddDays(3);

	[RelayCommand]
    void Clear()
    {
        SelectedDate = null;
    }

    [RelayCommand]
    async Task Success()
    {
        Closed?.Invoke(new CalendarPickerResult() { IsSuccess = SelectedDate.HasValue, SelectedDate = SelectedDate });

        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    async Task Cancel()
    {
        Closed?.Invoke(new CalendarPickerResult() { IsSuccess = false });
        await MopupService.Instance.PopAsync();
    }
}