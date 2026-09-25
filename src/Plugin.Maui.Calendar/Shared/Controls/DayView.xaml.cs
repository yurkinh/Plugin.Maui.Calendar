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
			// Source lets only the calendar that owns this cell handle the tap; the message is
			// broadcast to every calendar that is on screen.
			WeakReferenceMessenger.Default.Send(new DayTappedMessage(dayModel.Date) { Source = this });
        }
    }
}

