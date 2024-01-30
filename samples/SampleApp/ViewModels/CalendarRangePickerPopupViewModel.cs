﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Plugin.Maui.Calendar.Enums;
using SampleApp.Model;

namespace SampleApp.ViewModels;

public partial class CalendarRangePickerPopupViewModel : BasePageViewModel
{
	[ObservableProperty]
	DateTime maximumDate = DateTime.Today.AddYears(1);

	[ObservableProperty]
	DateTime minimumDate = DateTime.Today.AddYears(-1);

	[ObservableProperty]
	DateTime shownDate = DateTime.Today;

	[ObservableProperty]
	WeekLayout calendarLayout = WeekLayout.Month;

	[ObservableProperty]
	DateTime? selectedStartDate = DateTime.Today.AddDays(-5);

	[ObservableProperty]
	DateTime? selectedEndDate = DateTime.Today.AddDays(5);

	public event Action<CalendarRangePickerResult> Closed;

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
