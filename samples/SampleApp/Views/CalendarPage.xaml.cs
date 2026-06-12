using Plugin.Maui.Calendar.Models;

namespace SampleApp.Views;

public partial class CalendarPage : ContentPage
{
	public CalendarPage()
	{
		InitializeComponent();
		UpdateShownDatesLabel(CalendarControl.VisibleStartDate, CalendarControl.VisibleEndDate);
	}

	void OnShownDatesChanged(object sender, ShownDatesChangedEventArgs e)
	{
		UpdateShownDatesLabel(e.VisibleStartDate, e.VisibleEndDate);
	}

	void UpdateShownDatesLabel(DateTime startDate, DateTime endDate)
	{
		ShownDatesLabel.Text = $"Showing: {startDate:d} – {endDate:d}";
	}
}
