using Mopups.Services;
using Plugin.Maui.Calendar.Enums;
using SampleApp.Model;
using System.Windows.Input;

namespace SampleApp.ViewModels
{
    public class CalendarRangePickerPopupSelectedDatesViewModel : BasePageViewModel
    {
        private DateTime _maximumDate = DateTime.Today.AddYears(1);

        private DateTime _minimumDate = DateTime.Today.AddYears(-1);

        private DateTime _shownDate = DateTime.Today;

        private List<DateTime> _selectedDates = null;

        private WeekLayout _calendarLayout = WeekLayout.Month;

        public CalendarRangePickerPopupSelectedDatesViewModel()
        {
            SelectedDates = new List<DateTime>
            {
                DateTime.Today,
                DateTime.Today.AddDays(6),
            };
        }

        public event Action<CalendarRangePickerResult> Closed;

        public ICommand CancelCommand => new Command(async () =>
        {
            Closed?.Invoke(new CalendarRangePickerResult() { IsSuccess = false });
            await MopupService.Instance.PopAsync();
        });

        public ICommand ClearCommand => new Command(() =>
        {
            SelectedDates = null;
        });

        public DateTime MaximumDate
        {
            get => _maximumDate;
            set => SetProperty(ref _maximumDate, value);
        }

        public DateTime MinimumDate
        {
            get => _minimumDate;
            set => SetProperty(ref _minimumDate, value);
        }

        public DateTime ShownDate
        {
            get => _shownDate;
            set => SetProperty(ref _shownDate, value);
        }

        public WeekLayout CalendarLayout
        {
            get => _calendarLayout;
            set => SetProperty(ref _calendarLayout, value);
        }

        public List<DateTime> SelectedDates
        {
            get => _selectedDates;
            set => SetProperty(ref _selectedDates, value);
        }

        public ICommand SuccessCommand => new Command(async () =>
        {
            Closed?.Invoke(new CalendarRangePickerResult()
            {
                IsSuccess = true,
                SelectedDates = SelectedDates
            });
            await MopupService.Instance.PopAsync();
        });
    }
}
