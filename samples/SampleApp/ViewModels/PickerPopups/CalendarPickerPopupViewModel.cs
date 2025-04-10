﻿using Mopups.Services;
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
    DateTime shownDate = DateTime.Today;

    [ObservableProperty]
    WeekLayout calendarLayout = WeekLayout.Month;

    [ObservableProperty]
    DateTime? selectedDate;

    [ObservableProperty]
    DateTime minimumDate = new(1900, 1, 1);

    [ObservableProperty]
    DateTime maximumDate = DateTime.Today.AddDays(3);

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