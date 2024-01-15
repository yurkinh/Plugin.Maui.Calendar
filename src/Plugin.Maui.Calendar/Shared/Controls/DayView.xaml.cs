using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public partial class DayView : ContentView
{
	internal DayView()
	{
		InitializeComponent();        
	}   

    private void OnTapped(object sender, EventArgs e)
    {
        if (BindingContext is DayModel dayModel && !dayModel.IsDisabled && dayModel.IsVisible)
        {
            dayModel.IsSelected = !dayModel.IsSelected;
            dayModel.DayTappedCommand?.Execute(dayModel.Date);
        }
    }
}
