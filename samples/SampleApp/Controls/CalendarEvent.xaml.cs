using System.Windows.Input;

namespace SampleApp.Controls;

public partial class CalendarEvent : ContentView
{
    public static readonly BindableProperty CalendarEventCommandProperty =
        BindableProperty.Create(nameof(CalendarEventCommand), typeof(ICommand), typeof(CalendarEvent), null);

    public CalendarEvent()
    {
        InitializeComponent();
    }

    public ICommand CalendarEventCommand
    {
        get => (ICommand)GetValue(CalendarEventCommandProperty);
        set => SetValue(CalendarEventCommandProperty, value);
    }

    void TapGestureRecognizer_Tapped(object sender, System.EventArgs e)
    {
        if (BindingContext is AdvancedEventModel eventModel)
		{
			CalendarEventCommand?.Execute(eventModel);
		}
	}
}
