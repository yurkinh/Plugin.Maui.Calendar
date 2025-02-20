using CommunityToolkit.Mvvm.Messaging;
using Plugin.Maui.Calendar.Models;

namespace Plugin.Maui.Calendar.Controls;

public sealed partial class DayView : ContentView
{
	internal DayView()
	{
		InitializeComponent();
	}

    void OnTapped(object sender, EventArgs e)
    {
        if (BindingContext is DayModel dayModel && !dayModel.IsDisabled && dayModel.IsVisible)
        {
            if (!dayModel.AllowDeselect && dayModel.IsSelected)
            {
                return;
            }

            dayModel.IsSelected = !dayModel.IsSelected;
            dayModel.DayTappedCommand?.Execute(dayModel.Date);
			WeakReferenceMessenger.Default.Send(new DayTappedMessage(dayModel.Date));
        }
    }
}

